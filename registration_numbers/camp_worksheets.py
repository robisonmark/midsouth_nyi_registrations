import json
import re
from datetime import datetime
from typing import Any, OrderedDict

from config import API_KEY, EVENT, CAMP_FORM_ID, HOPE_CAMP_FORM_ID
from enums import Camp, Gender, RegistrationType
from FileManager import FileManager
from SwagManager import SwagManager
from SwagPDFGenerator import SwagPDFGenerator
from models.campers import Campers
from ShirtManager import ShirtManager
from translators.jotform import JotformClient
from translators.jotform_camper import translate_camper

# from functools import reduce


class CampWorksheets:
    def __init__(self):
        self._event = EVENT.CAMP
        self.file_manager = FileManager(self._event)
        self.shirt_manager = ShirtManager(level_specific=True)

        self.swag_manager = SwagManager(event_type="camp", age_group_specific=True, cutoff_date=None)
        self.swag_pdf_generator = SwagPDFGenerator(font_name="Helvetica", font_size=12)

        self.church_roster_worksheet = {}
        self.camp_master_roster = {
            "High School Camp": {"worksheet_name": "HS Participants", "data": []},
            "Middle School Camp": {"worksheet_name": "MS Participants", "data": []},
        }
        self.camp_master_entries = {
            "High School Camp": [],
            "Middle School Camp": [],
        }
        self.pricing_breakdown = {
            "student": {"05-03-26": 245, "05-17-26": 300, "05-24-26": 375},
            "chaperone": {
                "High School Camp": 150,
                "Middle School Camp": 150,
                "Both Camps": 250,
            },
        }
        self.late_fee = 0
        self.email_list = []
        self.shirts = []
        self.gotcha_participants = {
            "High School Camp": [],
            "Middle School Camp": [],
        }

    def read_file(self) -> list[OrderedDict]:
        files = self.file_manager.gather_files("./files")
        file_data = []
        for file in files:
            if "camp" in file.lower():
                file_data = self.file_manager.read_csv(file)
                sorted_data = sorted(
                    file_data,
                    key=lambda x: (
                        x.get("whatChurch", "Staff"),
                        x["whichCampStudent"] if x["whichCampStudent"] != "" else x["whichCampChaperone"],
                        x["yourName"]["first"],
                        x["yourName"]["first"],
                        x["submit_date_for_email_triggers"]["datetime"],
                    ),
                )
                return sorted_data
        return file_data

    def get_price(self, row):
        late_fee = 0

        if row.registration_type == RegistrationType.CHAPERONE:
            return self.pricing_breakdown["chaperone"][row.camp.value]

        if row.registration_type == RegistrationType.STAFF:
            return 0

        price_dates = self.pricing_breakdown["student"].keys()
        max_date = datetime.strptime(list(price_dates)[-1], "%m-%d-%y")

        for price_date_string in price_dates:
            if isinstance(row.registration_type_date, str):
                submission_date = datetime.strptime(row.registration_type_date, "%b %d, %Y")
            else:
                submission_date = row.registration_type_date

            price_date = datetime.strptime(price_date_string, "%m-%d-%y")
            if submission_date <= price_date:
                return self.pricing_breakdown["student"][price_date_string]

            elif submission_date > max_date:
                return self.pricing_breakdown["student"][list(price_dates)[-1]] + late_fee

    def youth_leader_email_list(self, row_data: dict) -> dict[str, list[str]]:
        if row_data.youth_leader_email == "" or row_data.youth_leader_email is None:
            return

        current_church = None
        email = row_data.youth_leader_email.lower()
        if current_church is None or current_church != row_data.church:
            current_church = row_data.church

            if email not in self.email_list:
                self.email_list.append(email)

    def create_church_worksheets(self, row_data: dict) -> None:
        current_church = None

        entry = {
            "approval_status": row_data.approval_status,
            "date": row_data.submission_date,
            "are_you_a_student_chaperone_staff": row_data.registration_type.value,
            "camp": row_data.camp.value,
            "first_name": row_data.first_name,
            "last_name": row_data.last_name,
            "paid_online": row_data.payment > 0.0,
            "price": self.get_price(row_data),
            "paid": row_data.payment,
        }

        if current_church is None or current_church != row_data.church:
            current_church = row_data.church

            # TODO: PULL THIS OUT INTO CREATION METHOD
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
                                    "Student, Chaperone, Staff",
                                    "Camp",
                                    "First Name",
                                    "Last Name",
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
            self.church_roster_worksheet[current_church][0]["data"].insert(
                row_index,
                {
                    "type": "row",
                    "row": row_index,
                    "col": 0,
                    "values": (
                        entry["approval_status"],
                        entry["date"],
                        entry["are_you_a_student_chaperone_staff"],
                        entry["camp"],
                        entry["first_name"],
                        entry["last_name"],
                        "",
                        entry["paid_online"],
                        entry["price"],
                        entry["paid"],
                        f"=SUM(I{row_index + 1}-J{row_index + 1})",
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
                    "col": 0,
                    "values": [
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "Total Due at Registration",
                        "",
                        f"=SUM(I2:I{str(current_row_index)})",
                        f"=SUM(J2:J{str(current_row_index)})",
                        f"=SUM(K2:K{str(current_row_index)})",
                    ],
                    "format": {"bold": True},
                },
            )
            self.church_roster_worksheet[church][0]["data"].insert(
                current_row_index + 1,
                {
                    "type": "row",
                    "row": current_row_index + 2,
                    "col": 0,
                    "values": ["", "", "", "", "", "", "Check Number", "", "", "Check Amount", ""],
                    "format": {"bold": True},
                },
            )

            self.file_manager.write_to_excel(
                "camp", f"{datetime.now().strftime('%Y_%m_%d')}_{church}", self.church_roster_worksheet[church]
            )

    def add_entry_to_camp(self, entry, camp_name):
        camp = camp_name.value
        student_pricing = list(self.pricing_breakdown["student"].values())
        chaperone_pricing = list(self.pricing_breakdown["chaperone"].values())

        if self.camp_master_roster[camp]["data"] == []:
            self.camp_master_roster[camp]["data"].append(
                {
                    "type": "header",
                    "row": 0,
                    "col": 0,
                    "values": [key.replace("_", " ").title() for key in entry.keys()],
                    "format": {"bg_color": "#bdbdbd", "bold": True},
                }
            )

        row_index = len(self.camp_master_roster[camp]["data"]) + 1
        entry["amount_owed"] = (
            f"=SUM((J{row_index} * {student_pricing[0]}) + (K{row_index} * {student_pricing[1]}) + (L{row_index} * {student_pricing[2]}) + (M{row_index} * {self.late_fee}) + (N{row_index} * {chaperone_pricing[0]}) + (O{row_index} * {chaperone_pricing[2]}))"
        )
        entry["balance"] = f"=SUM(Q{row_index} - R{row_index})"

        self.camp_master_entries[camp].append(dict(entry))

    def _camp_entry_sort_key(self, entry: dict[str, Any]) -> tuple[str, str]:
        church = (entry.get("church") or "").strip().lower()
        full_name = (entry.get("name") or "").strip()
        last_name = full_name.split()[-1].lower() if full_name else ""
        return church, last_name

    def _build_camp_master_sheet(self, camp: str) -> dict[str, Any]:
        header = {
            "type": "header",
            "row": 0,
            "col": 0,
            "values": [
                "Name",
                "Church",
                "Shirt Size",
                "Medical Release",
                "Tower Release",
                "Teen Female",
                "Teen Male",
                "Adult Female",
                "Adult Male",
                "First Deadline",
                "Second Deadline",
                "Final Deadline",
                "Late Fee",
                "Adult One Camp",
                "Adult Both Camps",
                "Pay Form",
                "Amount Owed",
                "Amount Paid",
                "Balance",
            ],
            "format": {"bg_color": "#bdbdbd", "bold": True},
        }

        rows = sorted(self.camp_master_entries[camp], key=self._camp_entry_sort_key)
        sheet_data = [header]

        student_pricing = list(self.pricing_breakdown["student"].values())
        chaperone_pricing = list(self.pricing_breakdown["chaperone"].values())

        for index, entry in enumerate(rows, start=1):
            amount_owed = (
                f"=SUM((J{index + 1} * {student_pricing[0]}) + "
                f"(K{index + 1} * {student_pricing[1]}) + "
                f"(L{index + 1} * {student_pricing[2]}) + "
                f"(M{index + 1} * {self.late_fee}) + "
                f"(N{index + 1} * {chaperone_pricing[0]}) + "
                f"(O{index + 1} * {chaperone_pricing[2]}))"
            )
            balance = f"=SUM(Q{index + 1} - R{index + 1})"

            row_values = [
                entry["name"],
                entry["church"],
                entry["shirt_size"],
                entry["medical_release"],
                entry["tower_release"],
                entry["teen_female"],
                entry["teen_male"],
                entry["adult_female"],
                entry["adult_male"],
                entry["first_deadline"],
                entry["second_deadline"],
                entry["final_deadline"],
                entry["late_fee"],
                entry["adult_one_camp"],
                entry["adult_both_camps"],
                entry["pay_form"],
                amount_owed,
                entry["amount_paid"],
                balance,
            ]

            sheet_data.append({"type": "row", "row": index, "col": 0, "values": row_values, "format": None})

        return {"worksheet_name": self.camp_master_roster[camp]["worksheet_name"], "data": sheet_data}

    def create_camp_master_worksheets(self, row_data: dict) -> None:
        camp = row_data.camp

        registration_date = row_data.registration_type_date
        teen = row_data.registration_type == RegistrationType.STUDENT
        adult = (
            row_data.registration_type == RegistrationType.CHAPERONE or row_data.registration_type == RegistrationType.STAFF
        )

        price_dates = self.pricing_breakdown["student"].keys()
        first_deadline = datetime.strptime(list(price_dates)[0], "%m-%d-%y")
        second_deadline = datetime.strptime(list(price_dates)[1], "%m-%d-%y")
        final_deadline = datetime.strptime(list(price_dates)[2], "%m-%d-%y")

        entry = {
            "name": f"{row_data.first_name} {row_data.last_name}",
            "church": row_data.church,
            "shirt_size": row_data.shirt_size.value,
            "medical_release": "",
            "tower_release": "",
            "teen_female": 1 if row_data.gender == Gender.FEMALE and teen else "",
            "teen_male": 1 if row_data.gender == Gender.MALE and teen else "",
            "adult_female": 1 if row_data.gender == Gender.FEMALE and adult else "",
            "adult_male": 1 if row_data.gender == Gender.MALE and adult else "",
            "first_deadline": 1 if teen and registration_date <= first_deadline else "",
            "second_deadline": 1 if teen and registration_date > first_deadline and registration_date <= second_deadline else "",
            "final_deadline": 1 if teen and registration_date > second_deadline else "",
            "late_fee": "1" if registration_date > final_deadline else "",
            "adult_one_camp": 1 if adult and camp != Camp.BOTH else "",
            "adult_both_camps": 1 if adult and camp == Camp.BOTH else "",
            "pay_form": "online" if row_data.payment > 0.00 else "",
            "amount_owed": self.get_price(row_data),
            "amount_paid": row_data.payment,
            "balance": "",
        }

        if camp == Camp.BOTH:
            self.add_entry_to_camp(entry, Camp.MIDDLE_SCHOOL)
            self.add_entry_to_camp(entry, Camp.HIGH_SCHOOL)
        else:
            self.add_entry_to_camp(entry, camp)


    def create_gotcha_spreadsheet(self, row_data: dict) -> None:
        camp = row_data.camp

        if row_data.gotcha:
            entry = {
                "name": f"{row_data.first_name} {row_data.last_name}",
                "church": row_data.church,
                "gotcha": True if row_data.gotcha.lower() == 'yes' else False,
            }

            if camp == Camp.BOTH:
                self.gotcha_participants[Camp.MIDDLE_SCHOOL.value].append(entry)
                self.gotcha_participants[Camp.HIGH_SCHOOL.value].append(entry)
            else:
                self.gotcha_participants[camp.value].append(entry)

    
    def create_gotcha_workbook(self) -> None:
        for camp in self.gotcha_participants:
            gotcha_sheet = {
                "worksheet_name": f"{camp} Gotcha",
                "data": [
                    {
                        "type": "header",
                        "row": 0,
                        "col": 0,
                        "values": ["Name", "Church", "Gotcha Participant", "", "Order"],
                        "format": {"bg_color": "#bdbdbd", "bold": True},
                    },
                    # {
                    #     "type": "row",
                    #     "row": 1,
                    #     "col": 4,
                    #     "values": [
                    #         "=SORT(FILTER(A2:A, C2:C=TRUE), RANDARRAY(COUNTA(FILTER(A2:A, C2:C=TRUE)), 1), TRUE)"
                    #     ],
                    #     "format": None,
                    # }
                ],
            }

            for index, entry in enumerate(self.gotcha_participants[camp], start=1):
                gotcha_sheet["data"].append(
                    {
                        "type": "row",
                        "row": index,
                        "col": 0,
                        "values": [entry["name"], entry["church"]],
                        "format": None,
                    }
                )
                gotcha_sheet["data"].append(
                    {
                        "type": "checkbox",
                        "row": index,
                        "col": 2,
                        "values": [entry["gotcha"]],
                        "format": None,
                    }
                )

            self.file_manager.write_to_excel("camp", f"{datetime.now().year}_{camp}_gotcha_participants", [gotcha_sheet])

    def create_camp_master_workbook(self) -> None:
        camp_sheets = {}
        for camp in self.camp_master_roster:
            master_sheet = self._build_camp_master_sheet(camp)
            current_row_index = len(master_sheet["data"])

            master_sheet["data"].append(
                {
                    "type": "row",
                    "row": current_row_index,
                    "col": 0,
                    "values": [
                        "Totals",
                        "",
                        "",
                        "",
                        "",
                        f"=SUM(F2:F{str(current_row_index)})",
                        f"=SUM(G2:G{str(current_row_index)})",
                        f"=SUM(H2:H{str(current_row_index)})",
                        f"=SUM(I2:I{str(current_row_index)})",
                        f"=SUM(J2:J{str(current_row_index)})",
                        f"=SUM(K2:K{str(current_row_index)})",
                        f"=SUM(L2:L{str(current_row_index)})",
                        f"=SUM(M2:M{str(current_row_index)})",
                        f"=SUM(N2:N{str(current_row_index)})",
                        f"=SUM(O2:O{str(current_row_index)})",
                        "",
                        f"=SUM(Q2:Q{str(current_row_index)})",
                        f"=SUM(R2:R{str(current_row_index)})",
                        f"=SUM(S2:S{str(current_row_index)})",
                    ],
                    "format": {"bold": True},
                },
            )
            master_sheet["data"].extend(
                [
                    {
                        "type": "col_format",
                        "first_col": 5,
                        "last_col": 5,
                        "width": 8,
                        "format": {"bg_color": "#ff85ff", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 6,
                        "last_col": 6,
                        "width": 8,
                        "format": {"bg_color": "#b4c6e7", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 7,
                        "last_col": 7,
                        "width": 8,
                        "format": {"bg_color": "#e394ff", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 8,
                        "last_col": 8,
                        "width": 8,
                        "format": {"bg_color": "#00b0f0", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 9,
                        "last_col": 9,
                        "width": 8,
                        "format": {"bg_color": "#92d050", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 10,
                        "last_col": 10,
                        "width": 8,
                        "format": {"bg_color": "#ffff00", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 11,
                        "last_col": 11,
                        "width": 8,
                        "format": {"bg_color": "#a64d79", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 12,
                        "last_col": 12,
                        "width": 8,
                        "format": {"bg_color": "#4472c4", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 13,
                        "last_col": 13,
                        "width": 8,
                        "format": {"bg_color": "#80c8c1", "align": "center"},
                    },
                    {
                        "type": "col_format",
                        "first_col": 13,
                        "last_col": 13,
                        "width": 8,
                        "format": {"bg_color": "#81a9a5", "align": "center"},
                    },
                ]
            )

            camp_sheets[camp] = master_sheet

        church_worksheets = []
        for church in sorted(self.church_roster_worksheet.keys(), key=lambda s: s.lower()):
            church_worksheets.append(self.church_roster_worksheet[church][0])

        self.camp_master_roster = [
            camp_sheets["High School Camp"],
            camp_sheets["Middle School Camp"],
            *church_worksheets,
        ]

        self.file_manager.write_to_excel("camp", f"{datetime.now().year}_camp_master", self.camp_master_roster)

    def create_shirt_master(self) -> None:
        for key, data_group in self.shirt_manager.get_shirt_roster.items():
            church_data = []
            i = 1
            for data in data_group:
                church_data.append({"type": "row", "row": i, "col": 0, "values": (data, "", "", ""), "format": None})
                i += 1

                for registrant_data in data_group[data]:
                    church_data.append(
                        {"type": "row", "row": i, "col": 0, "values": ("", *registrant_data.values()), "format": None}
                    )
                    i += 1

            worksheet_data = {
                "worksheet_name": key,
                "data": [
                    {
                        "type": "header",
                        "row": 0,
                        "col": 0,
                        "values": ["Church", "", "Name", "shirtSize"],
                        "format": {"bold": True},
                    },
                    *church_data,
                ],
            }
            self.shirts.append(worksheet_data)

        self.file_manager.write_to_excel("camp", f"{datetime.now().year}_shirt_roster", self.shirts)

    def generate_swag_pdfs(self) -> None:
        """Generate swag PDFs for all churches."""
        print("\n🎽 Generating Swag Reports...")
        swag_output = self.file_manager.create_directory("swag_reports")
        self.swag_pdf_generator.generate_all_pdfs(self.swag_manager, swag_output)
        print(f"✅ Swag reports saved to: {swag_output}")

    def process_data(self, raw_data: list[dict[str:str]]) -> list[dict[str:Any]]:
        online_payment = 0

        for data in raw_data:
            if data.approval_status.lower() not in ["deleted", "archived"]:
                self.swag_manager.add_registrant(data)

                if data.registration_type is RegistrationType.STAFF or data.church == "" or data.church is None:
                    data.church = "Staff"
                
                if data.church == "Memphis Hope Tabernacle":
                    data.church = "Hope Presbyterian"

                # could this be to create my row entries so I don't have to loop twice
                self.create_church_worksheets(data)
                self.create_gotcha_spreadsheet(data)
                self.create_camp_master_worksheets(data)
                self.youth_leader_email_list(data)
                
                #     #  =SORT(FILTER(A2:A, C2:C=TRUE), RANDARRAY(COUNTA(FILTER(A2:A, C2:C=TRUE)), 1), TRUE)

        self.create_church_workbook()
        self.create_gotcha_workbook()
        self.create_camp_master_workbook()
        # self.create_shirt_master()

        print(f"Total Online Payments: ${online_payment}")

        self.file_manager.write_to_txt_file("camp", "youth_leader_email", str(self.email_list))

        # Generate swag PDFs
        self.generate_swag_pdfs()

    # def generate_raw_data(self, submissions: list[dict[str:str]]) -> list[dict[str:Any]]:
    #     """
    #     Generates raw data from Jotform submissions.
    #     :param submissions: List of Jotform submissions.
    #     :return: Processed data ready for further processing.
    #     """
    #     processed_data = []

    #     for submission in submissions:
    #         registrant_submission = {
    #             "id": submission["id"],
    #             "submission_date": submission["created_at"],
    #             "status": submission["status"],
    #         }

    #         for answer in submission["answers"].values():
    #             if "answer" in answer.keys():
    #                 registrant_submission[answer["name"]] = answer["answer"]

    #         camper = translate_camper(registrant_submission)
    #         self.shirt_manager.create_individual_entry(row_data=camper)
    #         processed_data.append(camper)
        
    #     return processed_data


if __name__ == "__main__":
    run_camp_worksheets = CampWorksheets()

    jotform = JotformClient(API_KEY, CAMP_FORM_ID, translate_camper)
    camp_data = jotform.get_data()

    jotform_hope = JotformClient(API_KEY, HOPE_CAMP_FORM_ID, translate_camper)
    camp_data_hope = jotform_hope.get_data()

    camp_data.extend(camp_data_hope)

    run_camp_worksheets.process_data(camp_data)

   



    # TODO: Create Camp Gotcha Spreadsheet
    # TODO: CREATE TOWER RELEASE LIST
    # TODO: Create Camp Master (Start from Template - Guess Rooming)
    # TODO: Clean Up
    # TODO: CONFIRM SHIRT ROSTER 

