import re
from typing import OrderedDict, Any
from datetime import datetime

from jotform import JotformAPIClient

from config import EVENT, API_KEY
from FileManager import FileManager
from ShirtManager import ShirtManager
from functools import reduce


class CampWorksheets():
    def __init__(self):
        self._event = EVENT.CAMP
        self.file_manager = FileManager(self._event)
        self.shirt_manager = ShirtManager(level_specific=True)

        self.church_roster_worksheet = {}
        self.camp_master_roster = {
            "High School Camp": {
                "worksheet_name": "HS Participants",
                "data": []
            },
            "Middle School Camp": {
                "worksheet_name": "MS Participants",
                "data": []
            }
        }
        self.pricing_breakdown = {
            "student": {
                "05-02-25": 225,
                "05-16-25": 250,
                "05-23-25": 300
            },
            "chaperone": {
                "High School Camp": 150,
                "Middle School Camp": 150,
                "Both Camps": 250,
            }
        }
        self.late_fee = 0
        self.email_list = []
        self.shirts = []

    def read_file(self) -> list[OrderedDict]:
        files = self.file_manager.gather_files("./files")
        file_data = []
        for file in files:
            if "camp" in file.lower():
                file_data = self.file_manager.read_csv(file)
                sorted_data = \
                    sorted(file_data,
                           key=lambda x: (x.get('What church are you a part of?', 'Staff'),
                                          x['Which Camp?'] if x['Which Camp?'] != "" else 
                                          x['At which camp will you be a chaperone?'],
                                          x['First Name'],
                                          x['Last Name'],
                                          x['Date']))
                return sorted_data
        return file_data

    def get_price(self, row):
        late_fee = 0

        if row["Are you a student, chaperone, staff"] == "Chaperone":
            return self.pricing_breakdown["chaperone"][row["At which camp will you be a chaperone?"]]

        if row["Are you a student, chaperone, staff"] == "Staff":
            return 0

        submission_date_string = row['Date']
        price_dates = self.pricing_breakdown['student'].keys()
        max_date = datetime.strptime(list(price_dates)[-1], '%m-%d-%y')

        for price_date_string in price_dates:
            submission_date = datetime.strptime(submission_date_string, '%b %d, %Y')
            price_date = datetime.strptime(price_date_string, '%m-%d-%y')
            if submission_date <= price_date:
                return self.pricing_breakdown['student'][price_date_string]

            elif submission_date > max_date:
                return self.pricing_breakdown['student'][list(price_dates)[-1]] + late_fee

    def youth_leader_email_list(self, row_data: dict) -> dict[str, list[str]]:
        current_church = None
        email = row_data["What is your youth leader/pastor's email?"].lower()
        if current_church is None or current_church != row_data["What church are you a part of?"]:
            current_church = row_data["What church are you a part of?"]
            # if current_church not in self.email_list:
            # self.email_list[current_church] = []

            if email not in self.email_list:
                self.email_list.append(email)

    def create_church_worksheets(self, row_data: dict) -> None:
        current_church = None
        camp = row_data["Which Camp?"] if row_data["Which Camp?"] != "" else \
            row_data["At which camp will you be a chaperone?"]
        price = self.get_price(row_data)
        paid = int(re.findall("[0-9]+", row_data["Payment"])[0]) if re.findall("[0-9]+", row_data["Payment"]) else 0
        entry = {
            "approval_status": row_data["Flow Status"],
            "date": row_data['\ufeff"Submission Date"'],
            "are_you_a_student_chaperone_staff": row_data["Are you a student, chaperone, staff"],
            "camp": camp,
            "first_name": row_data["First Name"],
            "last_name": row_data["Last Name"],
            "paid_online": row_data["Payment"] != "",
            "price": price,
            "paid": paid,
        }

        if current_church is None or current_church != row_data["What church are you a part of?"]:
            current_church = row_data["What church are you a part of?"]

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
                                    "Total Due"
                                ],
                                "format": {"bold": True}
                            }
                        ]
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
                        f"=SUM(I{row_index + 1}-J{row_index + 1})"
                    ),
                    "format": None
                }
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
                    "values": ["",
                               "",
                               "",
                               "",
                               "",
                               "",
                               "Total Due at Registration",
                               "",
                               f"=SUM(I2:I{str(current_row_index)})",
                               f"=SUM(J2:J{str(current_row_index)})",
                               f"=SUM(K2:K{str(current_row_index)})"],
                    "format": {"bold": True}
                }
            )
            self.church_roster_worksheet[church][0]["data"].insert(
                current_row_index + 1,
                {
                    "type": "row",
                    "row": current_row_index + 2,
                    "col": 0,
                    "values": ["", "", "", "", "", "", "Check Number", "", "", "Check Amount", ""],
                    "format": {"bold": True}
                }
            )

            self.file_manager.write_to_excel("camp", f"{datetime.now().strftime('%Y_%m_%d')}_{church}", self.church_roster_worksheet[church])

    # Should this be a class method?
    def add_entry_to_camp(self, entry, camp_name):
        student_pricing = list(self.pricing_breakdown["student"].values())
        chaperone_pricing = list(self.pricing_breakdown["chaperone"].values())

        if self.camp_master_roster[camp_name]["data"] == []:
            self.camp_master_roster[camp_name]["data"].append(
                {
                    "type": "header",
                    "row": 0,
                    "col": 0,
                    "values": [key.replace("_", " ").title() for key in entry.keys()],
                    "format":  {"bg_color": "#bdbdbd", "bold": True}
                }
            )

        row_index = len(self.camp_master_roster[camp_name]["data"]) + 1
        entry["amount_owed"] = f"=SUM((J{row_index} * {student_pricing[0]}) +\
                (K{row_index} * {student_pricing[1]}) + (L{row_index}\
                * {student_pricing[2]}) + (M{row_index} * {self.late_fee}) +\
                (N{row_index} * {chaperone_pricing[0]}) + (O{row_index} *\
                {chaperone_pricing[1]}))"
        entry["balance"] = f"=SUM(Q{row_index} - R{row_index})"
        self.camp_master_roster[camp_name]["data"].insert(
            row_index - 1,
            {
                "type": "row",
                "row": row_index - 1,
                "col": 0,
                "values": list(entry.values()),
                "format": None
            }
        )

    def create_camp_master_worksheets(self, row_data: dict) -> None:
        camp = row_data["Which Camp?"] if row_data["Which Camp?"] != "" else \
            row_data["At which camp will you be a chaperone?"]

        registration_date = datetime.strptime(row_data["Date"], "%b %d, %Y")
        teen = row_data["Are you a student, chaperone, staff"] == "Student"
        adult = (row_data["Are you a student, chaperone, staff"] == "Chaperone"
                 or row_data["Are you a student, chaperone, staff"] == "Staff")
        
        entry = {
            "name": f"{row_data['First Name']} {row_data['Last Name']}",
            "church": row_data["What church are you a part of?"],
            "shirt_size": row_data["Shirt Size"],
            "medical_release": "",
            "tower_release": "",
            "teen_female": 1 if row_data["Gender"] == "Female" and teen else "",
            "teen_male": 1 if row_data["Gender"] == "Male" and teen else "",
            "adult_female": 1 if row_data["Gender"] == "Female" and adult else "",
            "adult_male": 1 if row_data["Gender"] == "Male" and adult else "",
            "first_deadline": 1 if teen and registration_date <= datetime(2025, 5, 2) else "",
            "second_deadline": 1 if teen and datetime(2025, 5, 2) < registration_date <= datetime(2025, 5, 16) else "",
            "final_deadline": 1 if teen and datetime(2025, 5, 16) < registration_date else "",
            "late_fee": "1" if registration_date > datetime(2025, 5, 23) else "",
            "adult_one_camp": 1 if adult and camp != "Both Camps" else "",
            "adult_both_camps": 1 if adult and camp == "Both Camps" else "",
            "pay_form": "online" if row_data["Payment"] != "" else "",
            "amount_owed": "",
            "amount_paid": int(re.findall("[0-9]+", row_data["Payment"])[0]) if
                    re.findall("[0-9]+", row_data["Payment"]) else 0,
            "balance": ""
        }

        if camp == "Both Camps":
            self.add_entry_to_camp(entry, "Middle School Camp")
            self.add_entry_to_camp(entry, "High School Camp")
        else:
            self.add_entry_to_camp(entry, camp)

    def create_camp_master_workbook(self) -> None:
        for camp in self.camp_master_roster:
            current_row_index = len(self.camp_master_roster[camp]["data"])
            self.camp_master_roster[camp]["data"].insert(
                len(self.camp_master_roster[camp]["data"]),
                {
                    "type": "row",
                    "row": current_row_index,
                    "col": 0,
                    "values": ["Totals",
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
                               f"=SUM(S2:S{str(current_row_index)})"],
                    "format": {"bold": True}
                }
            )
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 5,
                "last_col": 5,
                "width": 8,
                "format": {"bg_color": "#ff85ff", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 6,
                "last_col": 6,
                "width": 8,
                "format": {"bg_color": "#b4c6e7", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 7,
                "last_col": 7,
                "width": 8,
                "format": {"bg_color": "#e394ff", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 8,
                "last_col": 8,
                "width": 8,
                "format": {"bg_color": "#00b0f0", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 9,
                "last_col": 9,
                "width": 8,
                "format": {"bg_color": "#92d050", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 10,
                "last_col": 10,
                "width": 8,
                "format": {"bg_color": "#ffff00", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 11,
                "last_col": 11,
                "width": 8,
                "format": {"bg_color": "#a64d79", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 12,
                "last_col": 12,
                "width": 8,
                "format": {"bg_color": "#4472c4", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 13,
                "last_col": 13,
                "width": 8,
                "format": {"bg_color": "#80c8c1", "align": "center"}
            })
            self.camp_master_roster[camp]["data"].append({
                "type": "col_format",
                "first_col": 13,
                "last_col": 13,
                "width": 8,
                "format": {"bg_color": "#81a9a5", "align": "center"}
            })

        church_worksheets = [self.church_roster_worksheet[church][0] for church in self.church_roster_worksheet.keys()]

        self.camp_master_roster = [
            self.camp_master_roster['High School Camp'],
            self.camp_master_roster['Middle School Camp'],
            *church_worksheets
        ]

        self.file_manager.write_to_excel("camp", f"{datetime.now().year}_camp_master", self.camp_master_roster)

    def create_shirt_master(self) -> None:
        for key, data_group in self.shirt_manager.get_shirt_roster.items():
            church_data = []
            i = 1
            for data in data_group:
                church_data.append(
                    {"type": "row", "row": i, "col": 0, "values": (data, "", "", ""), "format": None}
                )
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
                        "values": ["Church", "", "Name", "Shirt Size"],
                        "format": {"bold": True}
                    },
                    *church_data
                ]
            }
            self.shirts.append(worksheet_data)

        self.file_manager.write_to_excel("camp", f"{datetime.now().year}_shirt_roster", self.shirts)

    def process_data(self, raw_data: list[dict[str: str]]) -> list[dict[str: Any]]:
        online_payment = 0
        for data in raw_data:
            if data["What church are you a part of?"] == "":
                data["What church are you a part of?"] = "Staff"
            
            if data["Payment"] != "":
                online_payment += int(re.findall("[0-9]+", data["Payment"])[0]) if re.findall("[0-9]+", data["Payment"]) else 0

            # could this be to create my row entries so I don't have to loop twice
            self.shirt_manager.create_individual_entry(row_data=data)
            self.create_church_worksheets(data)
            self.create_camp_master_worksheets(data)
            self.youth_leader_email_list(data)

        self.create_church_workbook()
        self.create_camp_master_workbook()
        self.create_shirt_master()

        print(f"Total Online Payments: ${online_payment}")

        self.file_manager.write_to_txt_file("camp", "youth_leader_email", str(self.email_list))

    def generate_raw_data(self, submissions: list[dict[str: str]]) -> list[dict[str: Any]]:
        """
        Generates raw data from Jotform submissions.
        :param submissions: List of Jotform submissions.
        :return: Processed data ready for further processing.
        """
        processed_data = []

        # def reduce_submission(accumulator, submission):
        #     row_data = []
        #     registrant_submission = {}
        #     if "answer" in submission.keys():
        #         registrant_submission[submission["name"]] = submission["answer"]

        #     accumulator.append(row_data)
        #     return accumulator
        for submission in submissions:
            # result = reduce(reduce_submission, submission['answers'].values(), [])
            registrant_submission = {}
            for answer in submission['answers'].values():
                if "answer" in answer.keys():
                    registrant_submission[answer["name"]] = answer["answer"]
                
            processed_data.append(registrant_submission)

        return processed_data


if __name__ == "__main__":
    run_camp_worksheets = CampWorksheets()
    # camp_data = run_camp_worksheets.read_file()
    # run_camp_worksheets.process_data(camp_data)
    jotform = JotformAPIClient(API_KEY)
    submissions = jotform.get_form_submissions("250758652718164", limit=10)
    camp_data = run_camp_worksheets.generate_raw_data(submissions)
    print(camp_data)

    # print(camp_data[0])
    # TODO: Gather Each Church Roster to export into xlsx
    # TODO: Separate Middle School and High School Camps, Making Sure to Only Charge Sponsors 1 time
    # TODO: Create Shirt Master
    # TODO: Create TNU Master
    # TODO: Create Camp Master (Start from Template - Guess Rooming)
