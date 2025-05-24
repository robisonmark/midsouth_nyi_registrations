import xlrd2
import os
import csv

import datetime

# loop column headers and find index?
STUDENT_CHAPERONE_COLUMN = 3
REGISTRATION_DATE_COLUMN = 2
FIRST_NAME_COLUMN = 6
LAST_NAME_COLUMN = 7
CHURCH_COLUMN = 32
SHIRT_SIZE_COLUMN = 17
GENDER_COLUMN = 16
STUDENT_CAMP_COLUMN = 4
CHAPERONE_CAMP_COLUMN = 5
PAYMENT_COLUMN = 48
CAMP_COLUMN = 52

CAMPS = {"high_school": [], "middle_school": []}


hs_output_list = []
ms_output_list = []
file_path = "./church/2025-05-17/"
for root, dirs, files in os.walk(file_path, topdown=False):
    for name in files:
        print(os.path.join(root, name))
        if name == ".DS_Store":
            continue
        workbook_file = os.path.join(root, name)
        book = xlrd2.open_workbook(workbook_file)
        print("The number of worksheets is {0}".format(book.nsheets))
        print("Worksheet name(s): {0}".format(book.sheet_names()))
        sh = book.sheet_by_index(0)
        #   print("{0} {1} {2}".format(sh.name, sh.nrows, sh.ncols))
        #   print("Cell D30 is {0}".format(sh.cell_value(rowx=2, colx=3)))
        for rx in range(sh.nrows - 1):
            # What is this checking
            if sh.cell_value(rowx=rx + 1, colx=LAST_NAME_COLUMN) == "":
                continue
            given_name = sh.cell_value(rowx=rx + 1, colx=FIRST_NAME_COLUMN)
            family_name = sh.cell_value(rowx=rx + 1, colx=LAST_NAME_COLUMN)
            name = f"{family_name}, {given_name}"
            church = sh.cell_value(rowx=rx + 1, colx=CHURCH_COLUMN)
            shirt = sh.cell_value(rowx=rx + 1, colx=SHIRT_SIZE_COLUMN)
            mr = ""
            tower = ""
            tf = (
                1
                if sh.cell_value(rowx=rx + 1, colx=GENDER_COLUMN) == "Female"
                and sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Student"
                else ""
            )
            tm = (
                1
                if sh.cell_value(rowx=rx + 1, colx=GENDER_COLUMN) == "Male"
                and sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Student"
                else ""
            )
            af = (
                1
                if sh.cell_value(rowx=rx + 1, colx=GENDER_COLUMN) == "Female"
                and (sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                in ["Chaperone", "Staff"] or sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Staff")
                else ""
            )
            am = (
                1
                if sh.cell_value(rowx=rx + 1, colx=GENDER_COLUMN) == "Male"
                and (sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                in ["Chaperone", "Staff"] or sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Staff")
                else ""
            )
            datestr = sh.cell_value(rowx=rx + 1, colx=REGISTRATION_DATE_COLUMN)
            print(datestr)
            date_parse = datetime.datetime.strptime(datestr, "%b %d, %Y")
            print(date_parse)
            first_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                not in ["Chaperone", "Staff"]
                and date_parse <= datetime.datetime(2024, 5, 10)
                else ""
            )
            second_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                not in ["Chaperone", "Staff"]
                and date_parse > datetime.datetime(2024, 5, 10)
                and date_parse <= datetime.datetime(2024, 5, 24)
                else ""
            )
            late_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                not in ["Chaperone", "Staff"]
                and date_parse > datetime.datetime(2024, 5, 24)
                else ""
            )
            late_fee = 0 if date_parse > datetime.datetime(2024, 5, 31) else ""
            adult_one = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                in ["Chaperone", "Staff"]
                and sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower() != "both camps"
                else ""
            )
            adult_both = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                in ["Chaperone", "Staff"]
                and sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower() == "both camps"
                else ""
            )
            pay_form = (
                "online"
                if sh.cell_value(rowx=rx + 1, colx=PAYMENT_COLUMN) != ""
                else ""
            )
            print(sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower())
            if (
                sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower()
                == "high school camp"
            ):
                CAMPS["high_school"].append(
                    (
                        name,
                        church,
                        shirt,
                        mr,
                        tower,
                        tf,
                        tm,
                        af,
                        am,
                        first_date,
                        second_date,
                        late_date,
                        late_fee,
                        adult_one,
                        adult_both,
                        pay_form,
                    )
                )
            elif sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower() == "both camps":
                CAMPS["high_school"].append(
                    (
                        name,
                        church,
                        shirt,
                        mr,
                        tower,
                        tf,
                        tm,
                        af,
                        am,
                        first_date,
                        second_date,
                        late_date,
                        late_fee,
                        adult_one,
                        adult_both,
                        pay_form,
                    )
                )
                CAMPS["middle_school"].append(
                    (
                        name,
                        church,
                        shirt,
                        mr,
                        tower,
                        tf,
                        tm,
                        af,
                        am,
                        first_date,
                        second_date,
                        late_date,
                        late_fee,
                        adult_one,
                        adult_both,
                        pay_form,
                    )
                )
            else:
                CAMPS["middle_school"].append(
                    (
                        name,
                        church,
                        shirt,
                        mr,
                        tower,
                        tf,
                        tm,
                        af,
                        am,
                        first_date,
                        second_date,
                        late_date,
                        late_fee,
                        adult_one,
                        adult_both,
                        pay_form,
                    )
                )


for camp_group in CAMPS.keys():
    with open(
        f"./church/main_roster_sheet_{camp_group}.csv", "w", newline=""
    ) as csvfile:
        spamwriter = csv.writer(csvfile, delimiter=",")
        for i in CAMPS[camp_group]:
            spamwriter.writerow(i)
