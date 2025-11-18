import json
import os
from datetime import datetime
from typing import Any, OrderedDict

from config import API_KEY, EVENT, MOMENTUM_FORM_ID
from dotenv import dotenv_values
from enums import ParticipationStatus, RegistrationType
from FileManager import FileManager
from SwagManager import SwagManager
from SwagPDFGenerator import SwagPDFGenerator
from translators.jotform import JotformClient
from translators.jotform_momentum import translate_registrant

config = {**dotenv_values("./.env")}

api_key = config.get("JOTFORM_API_KEY")


class MomentumWorksheets:
    """
    Class to handle the generation of raw data from Jotform submissions for Momentum worksheets.
    """

    def __init__(self):
        self._event = EVENT.MOMENTUM
        self.file_manager = FileManager(self._event)

        # Initialize SwagManager for Momentum (no age group split)
        # Cutoff date: November 4, 2025 (last guaranteed date)
        self.swag_manager = SwagManager(event_type="momentum", age_group_specific=False, cutoff_date=datetime(2025, 11, 4))
        self.swag_pdf_generator = SwagPDFGenerator(font_name="Helvetica", font_size=12)

        self.church_roster_worksheet = {}
        self.church_shirt_worksheet = {}
        self.event_roster_by_category = {}
        self.church_summary_worksheet = {}
        self.momentum_main_worksheet = {}

        # Define pricing as when price expires. The last date defines the walk up price
        self.pricing_breakdown = {
            "student": {"10-24-25": 85, "11-14-25": 130, "11-15-25": 175},
            "chaperone": 35,
            "spectator": 55,
        }
        self.late_fee = 0

    def get_price(self, row):
        if row.registration_type == RegistrationType.CHAPERONE:
            return self.pricing_breakdown["chaperone"]

        if row.registration_type == RegistrationType.STAFF:
            return 0

        price_dates = self.pricing_breakdown["student"].keys()

        if row.participation_status == ParticipationStatus.SPECTATOR:
            return self.pricing_breakdown["spectator"]

        max_date = datetime.strptime(list(price_dates)[-1], "%m-%d-%y")

        for price_date_string in price_dates:
            if isinstance(row.registration_type_date, str):
                submission_date = datetime.strptime(row.registration_type_date, "%b %d, %Y")
            else:
                submission_date = row.registration_type_date

            price_date = datetime.strptime(price_date_string, "%m-%d-%y")

            if submission_date <= price_date:
                return self.pricing_breakdown["student"][price_date_string]

            elif submission_date >= max_date:
                return self.pricing_breakdown["student"][list(price_dates)[-1]] + self.late_fee

    def create_church_worksheets(self, row_data: dict) -> None:
        current_church = None

        entry = {
            "approval_status": row_data.approval_status,
            "date": row_data.submission_date,
            "first_name": row_data.first_name,
            "last_name": row_data.last_name,
            "shirt_size": row_data.shirt_size,
            "gender": row_data.gender,
            "registration_type": row_data.registration_type,
            "participation_status": row_data.participation_status,
            "grade_level": row_data.grade_level,
            "age_range_talent": row_data.age_range_talent,
            "age_range_individual": row_data.age_range_individual,
            "art_events": row_data.art_events,
            "academic": row_data.academic_events,
            "creative_ministries": row_data.creative_ministries_events,
            "music": row_data.music_events,
            "individual_sports": row_data.individual_sports_events,
            "team_sports_events": row_data.team_sports_events,
            "paid_online": row_data.payment > 0.0,
            "price": row_data.payment,
            "paid": row_data.payment,
        }

        if current_church is None or current_church != row_data.church:
            current_church = row_data.church

            if self.church_roster_worksheet.get(current_church) is None:
                self.church_roster_worksheet[current_church] = [
                    {
                        "worksheet_name": current_church if len(current_church) < 30 else current_church[:30],
                        "data": [
                            {
                                "type": "header",
                                "row": 0,
                                "col": 0,
                                "values": [
                                    "Approval Status",
                                    "Form Submission Date",
                                    "Registration Type",
                                    "Participation Status",
                                    "First Name",
                                    "last Name",
                                    "Shirt Size",
                                    "",
                                    "Events",
                                    "Arts",
                                    "Academics",
                                    "Creative Ministries",
                                    "Music",
                                    "Individual Sports",
                                    "Team Sports",
                                    "Event Errors",
                                    "",
                                    "Paid Online?",
                                    "Price",
                                    "Paid",
                                    "Total Due",
                                ],
                                "format": {"bold": True},
                            }
                        ],
                    }
                ]

            row_index = len(self.church_roster_worksheet[current_church][0]["data"])

            art_events = ", ".join([member.value for member in row_data.art_events])
            academic_events = ", ".join([member.value for member in row_data.academic_events])
            creative_ministries_events = ", ".join([member.value for member in row_data.creative_ministries_events])
            music_events = ", ".join([member.value for member in row_data.music_events])
            individual_sports_events = ", ".join([member.value for member in row_data.individual_sports_events])
            team_sports_events = ", ".join([member.value for member in row_data.team_sports_events])
            event_errors = ", ".join([member for member in row_data.event_errors])

            self.church_roster_worksheet[current_church][0]["data"].insert(
                row_index,
                {
                    "type": "row",
                    "row": row_index,
                    "col": 0,
                    "values": (
                        row_data.approval_status,
                        str(row_data.submission_date.date()),
                        row_data.registration_type.value,
                        row_data.participation_status.value,
                        row_data.first_name,
                        row_data.last_name,
                        row_data.shirt_size.value,
                        "",
                        "",
                        art_events,
                        academic_events,
                        creative_ministries_events,
                        music_events,
                        individual_sports_events,
                        team_sports_events,
                        event_errors,
                        "",
                        str(row_data.payment > 0.0),
                        self.get_price(row_data),
                        entry["paid"],
                        f"=SUM(S{row_index + 1}-T{row_index + 1})",
                    ),
                    "format": None,
                },
            )

    def create_church_workbook(self) -> None:
        for church in self.church_roster_worksheet:
            current_row_index = len(self.church_roster_worksheet[church][0]["data"])
            self.church_roster_worksheet[church][0]["data"].insert(
                len(self.church_roster_worksheet[church][0]["data"]),
                {
                    "type": "row",
                    "row": current_row_index,
                    "col": 16,
                    "values": [
                        "Total Due at Registration",
                        "",
                        f"=SUM(S2:S{str(current_row_index)})",
                        f"=SUM(T2:T{str(current_row_index)})",
                        f"=SUM(U2:U{str(current_row_index)})",
                    ],
                    "format": {"bold": True},
                },
            )
            self.church_roster_worksheet[church][0]["data"].insert(
                len(self.church_roster_worksheet[church][0]["data"]),
                {
                    "type": "row",
                    "row": 0,
                    "col": 22,
                    "values": ["Payment Details", "", "", "Total Due", f"=U{current_row_index + 1}"],
                    "format": {"bold": True},
                },
            )

            self.church_roster_worksheet[church][0]["data"].insert(
                current_row_index + 1,
                {
                    "type": "row",
                    "row": 1,
                    "col": 22,
                    "values": ["Check Number", "", "", "Check Amount", ""],
                    "format": {"bold": True},
                },
            )

            self.file_manager.write_to_excel(
                "church_roster",
                f"{datetime.now().strftime('%Y_%m_%d')}_{church}",
                self.church_roster_worksheet[church],
                church=church,
            )

    def create_roster_by_category_worksheets(self, row_data: dict) -> None:
        art_events = row_data.art_events
        academic_events = row_data.academic_events
        creative_ministries_events = row_data.creative_ministries_events
        music_events = row_data.music_events
        individual_sports_events = row_data.individual_sports_events
        team_sports_events = row_data.team_sports_events

        participating_in = {
            "art": art_events,
            "academic": academic_events,
            "creative_ministries": creative_ministries_events,
            "music": music_events,
            "individual_sports": individual_sports_events,
            "team_sport": team_sports_events,
        }

        entry = {
            "first_name": row_data.first_name,
            "last_name": row_data.last_name,
            "church": row_data.church,
            "cell_phone": row_data.cell_phone,
        }
        if row_data.participation_status == ParticipationStatus.PARTICIPANT:
            for category, values in participating_in.items():
                for event_enum in values:
                    event = event_enum.value
                    if self.event_roster_by_category.get(category) is None:
                        self.event_roster_by_category[category] = {}

                    if self.event_roster_by_category[category].get(event) is None:
                        excel_safe_name = event.replace("/", "-").replace("\\", "-")

                        # Names for these too are too long for Excel worksheets
                        if excel_safe_name == "Human Video-Interpretive Worship Group":
                            excel_safe_name = "Human Video - Group"
                        if excel_safe_name == "Human Video-Interpretive Worship Solo":
                            excel_safe_name = "Human Video - Solo"

                        self.event_roster_by_category[category][event] = [
                            {
                                "worksheet_name": excel_safe_name if len(excel_safe_name) < 30 else excel_safe_name[:30],
                                "data": [
                                    {
                                        "type": "header",
                                        "row": 0,
                                        "col": 0,
                                        "values": ["First Name", "Last Name", "Church", "Cell Phone"],
                                        "format": {"bold": True},
                                    }
                                ],
                            }
                        ]

                    row_index = len(self.event_roster_by_category[category][event][0]["data"])
                    self.event_roster_by_category[category][event][0]["data"].insert(
                        row_index,
                        {
                            "type": "row",
                            "row": row_index,
                            "col": 0,
                            "values": (entry["first_name"], entry["last_name"], entry["church"], entry["cell_phone"]),
                            "format": None,
                        },
                    )

    def create_roster_workbook(self) -> None:
        for category, events in self.event_roster_by_category.items():
            worksheet_values = [event[0] for key, event in events.items()]

            self.file_manager.write_to_excel(
                "event_roster",
                f"{datetime.now().strftime('%Y_%m_%d')}_{category}",
                worksheet_values,
            )

    def create_church_summary(self, row_data: dict) -> None:
        current_church = None

        if current_church is None or current_church != row_data.church:
            current_church = row_data.church

            if self.church_summary_worksheet.get(current_church) is None:
                self.church_summary_worksheet[current_church] = []

            self.church_summary_worksheet[current_church].append(row_data.first_name + " " + row_data.last_name)

    def create_church_summary_pdf(self) -> None:
        data = self.church_summary_worksheet
        self.file_manager.create_pdf_from_dict(data, "summaries.pdf")

    def aggregate_momentum_main_sheet(self, row_data: dict) -> None:
        current_church = None

        if current_church is None or current_church != row_data.church:
            current_church = row_data.church

            if self.momentum_main_worksheet.get(current_church) is None:
                self.momentum_main_worksheet[current_church] = [
                    {
                        "worksheet_name": current_church if len(current_church) < 30 else current_church[:30],
                        "data": [
                            {
                                "type": "header",
                                "row": 0,
                                "col": 0,
                                "values": [
                                    "Uploaded to TNT",
                                    "Church",
                                    "First Name",
                                    "last Name",
                                    "Shirt Size",
                                    "Gender",
                                    "Registration Type",
                                    "Participation Status",
                                    "Grade Level",
                                    "Address",
                                    "",
                                    "Events",
                                    "Arts",
                                    "Academics",
                                    "Creative Ministries",
                                    "Music",
                                    "Individual Sports",
                                    "Team Sports",
                                    "Event Errors",
                                ],
                                "format": {"bold": True},
                            }
                        ],
                    }
                ]

            row_index = len(self.momentum_main_worksheet[current_church][0]["data"])

            art_events = ", ".join([member.value for member in row_data.art_events])
            academic_events = ", ".join([member.value for member in row_data.academic_events])
            creative_ministries_events = ", ".join([member.value for member in row_data.creative_ministries_events])
            music_events = ", ".join([member.value for member in row_data.music_events])
            individual_sports_events = ", ".join([member.value for member in row_data.individual_sports_events])
            team_sports_events = ", ".join([member.value for member in row_data.team_sports_events])
            event_errors = ", ".join([member for member in row_data.event_errors])

            address2 = f"{row_data.street_address2}," if row_data.street_address2 != "" else ""
            address = f"{row_data.street_address}, {address2} {row_data.city}, {row_data.state} {row_data.zip_code}"

            self.momentum_main_worksheet[current_church][0]["data"].insert(
                row_index,
                {
                    "type": "row",
                    "row": row_index,
                    "col": 0,
                    "values": (
                        False,
                        row_data.church,
                        row_data.first_name,
                        row_data.last_name,
                        row_data.shirt_size.value,
                        row_data.gender.value,
                        row_data.registration_type.value,
                        row_data.participation_status.value,
                        row_data.grade_level,
                        address,
                        "",
                        art_events,
                        academic_events,
                        creative_ministries_events,
                        music_events,
                        individual_sports_events,
                        team_sports_events,
                        event_errors,
                    ),
                    "format": None,
                },
            )

    def create_momentum_main_workbook(self) -> None:
        all_rows = [
            {
                "worksheet_name": "Momentum Main",
                "data": [
                    {
                        "type": "header",
                        "row": 0,
                        "col": 0,
                        "values": [
                            "Uploaded to TNT",
                            "Church",
                            "First Name",
                            "last Name",
                            "Gender",
                            "Registration Type",
                            "Participation Status",
                            "Grade Level",
                            "",
                            "Events",
                            "Arts",
                            "Academics",
                            "Creative Ministries",
                            "Music",
                            "Individual Sports",
                            "Team Sports",
                            "Event Errors",
                        ],
                        "format": {"bold": True},
                    }
                ],
            }
        ]

        # Iterate churches in alphabetical order and sort each sheet's rows by last name
        for church in sorted(self.momentum_main_worksheet.keys(), key=lambda s: s.lower()):
            sheet = self.momentum_main_worksheet[church][0]

            data = sheet.get("data", [])
            if not data:
                continue

            header = data[0]
            rows = data[1:]

            # Sort rows by last name (values[3])
            def _last_name_key(row):
                vals = row.get("values", ())
                try:
                    return (str(vals[3]) or "").strip().lower()
                except Exception:
                    return ""

            rows.sort(key=_last_name_key)

            sheet["data"] = [header] + rows

            all_rows[0]["data"].extend(sheet["data"][1:])

        # Re-index row numbers
        for idx, r in enumerate(all_rows[0]["data"], start=1):
            r["row"] = idx

        self.file_manager.write_to_excel(
            "momentum_main_sheet", f"{datetime.now().strftime('%Y_%m_%d')}_momentum_main", all_rows
        )

    def generate_swag_pdfs(self) -> None:
        """Generate swag PDFs for all churches."""
        print("\n🎽 Generating Swag Reports...")
        swag_output = self.file_manager.create_directory("swag_reports")
        self.swag_pdf_generator.generate_all_pdfs(self.swag_manager, swag_output)
        print(f"✅ Swag reports saved to: {swag_output}")

    def process_data(self, raw_data: list[dict[str:Any]]) -> None:
        """
        Processes the raw data and generates the necessary worksheets.
        :param raw_data: List of registrant data from Jotform.
        """
        for data in raw_data:
            if data.approval_status.lower() not in ["deleted", "archived"]:
                # Add to swag manager
                self.swag_manager.add_registrant(data)

                if data.church == "" or data.registration_type is RegistrationType.STAFF:
                    data.church = "Staff"

                self.create_church_worksheets(data)
                self.create_roster_by_category_worksheets(data)
                self.create_church_summary(data)
                self.aggregate_momentum_main_sheet(data)

        self.create_church_workbook()
        self.create_roster_workbook()
        self.create_church_summary_pdf()
        self.create_momentum_main_workbook()

        # Generate swag PDFs
        self.generate_swag_pdfs()


if __name__ == "__main__":
    momentum_worksheets = MomentumWorksheets()
    jotform = JotformClient(api_key, MOMENTUM_FORM_ID, translate_registrant)

    momentum_data = jotform.get_data()
    momentum_worksheets.process_data(momentum_data)
