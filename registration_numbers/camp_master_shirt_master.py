import xlrd2
import os
import csv
import enum

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


class AGE_GROUP:
    HIGH_SCHOOL = "high_school"
    MIDDLE_SCHOOL = "middle_school"


CAMPS = {
    "high_school": {},
    "middle_school": {},
}
church_count = {
    "high_school": {},
    "middle_school": {},
}


class Church:
    def create_church_entry(age_group: AGE_GROUP, church: str):
        if church not in CAMPS[age_group]:
            CAMPS[age_group][church] = []

    def add_to_church_count(age_group: AGE_GROUP, church: str):
        if church not in church_count[age_group]:
            church_count[age_group][church] = 1
        else:
            church_count[age_group][church] += 1


file_path = "./church/2025-05-17/"
for root, dirs, files in os.walk(file_path, topdown=False):
    for name in files:
        # print(os.path.join(root, name))
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
                and sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Chaperone"
                else ""
            )
            am = (
                1
                if sh.cell_value(rowx=rx + 1, colx=GENDER_COLUMN) == "Male"
                and sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Chaperone"
                else ""
            )
            datestr = sh.cell_value(rowx=rx + 1, colx=REGISTRATION_DATE_COLUMN)
            date_parse = datetime.datetime.strptime(datestr, "%b %d, %Y")
            first_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                != "Chaperone"
                and date_parse <= datetime.datetime(2024, 5, 10)
                else ""
            )
            second_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                != "Chaperone"
                and date_parse > datetime.datetime(2024, 5, 10)
                and date_parse <= datetime.datetime(2024, 5, 24)
                else ""
            )
            late_date = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                != "Chaperone"
                and date_parse > datetime.datetime(2024, 5, 24)
                else ""
            )
            # late_fee = 250 if date_parse > datetime.datetime(2023, 6, 1) else ''
            adult_one = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Chaperone"
                and sh.cell_value(rowx=rx + 1, colx=CHAPERONE_CAMP_COLUMN).lower()
                != "both camps"
                else ""
            )
            adult_both = (
                1
                if sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)
                == "Chaperone"
                and sh.cell_value(rowx=rx + 1, colx=CHAPERONE_CAMP_COLUMN).lower()
                == "both camps"
                else ""
            )
            pay_form = (
                "online"
                if sh.cell_value(rowx=rx + 1, colx=PAYMENT_COLUMN) != ""
                else ""
            )
            # print(sh.cell_value(rowx=rx+1, colx=6).lower())

            if (
                sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower()
                == "high school camp"
            ):
                Church.add_to_church_count(AGE_GROUP.HIGH_SCHOOL, church)
                Church.create_church_entry(AGE_GROUP.HIGH_SCHOOL, church)

                CAMPS["high_school"][church].append(
                    {
                        "no": len(CAMPS["high_school"][church]) + 1,
                        "Name": name,
                        "Shirt Size": shirt,
                    }
                )
            elif sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower() == "both camps":
                Church.add_to_church_count(AGE_GROUP.HIGH_SCHOOL, church)
                Church.add_to_church_count(AGE_GROUP.MIDDLE_SCHOOL, church)

                Church.create_church_entry(AGE_GROUP.HIGH_SCHOOL, church)
                Church.create_church_entry(AGE_GROUP.MIDDLE_SCHOOL, church)

                CAMPS["high_school"][church].append(
                    {
                        "no": len(CAMPS["high_school"][church]) + 1,
                        "Name": name,
                        "Shirt Size": shirt,
                    }
                )

                CAMPS["middle_school"][church].append(
                    {
                        "no": len(CAMPS["middle_school"][church]) + 1,
                        "Name": name,
                        "Shirt Size": shirt,
                    }
                )
            else:
                Church.add_to_church_count(AGE_GROUP.MIDDLE_SCHOOL, church)
                Church.create_church_entry(AGE_GROUP.MIDDLE_SCHOOL, church)

                if church not in CAMPS["middle_school"]:
                    CAMPS["middle_school"][church] = []
                CAMPS["middle_school"][church].append(
                    {
                        "no": len(CAMPS["middle_school"][church]) + 1,
                        "Name": name,
                        "Shirt Size": shirt,
                    }
                )


def sort_shirts_by_size(a):
    if a == "Small":
        return -2
    elif a == "Medium":
        return -1
    elif a == "Large":
        return 0
    elif a == "XL":
        return 1
    else:
        return int(a.split("X")[0])


for camp_group in CAMPS.keys():
    with open(f"./church/shirt_master_{camp_group}.csv", "w", newline="") as csvfile:
        writer = csv.writer(csvfile)
        for key, value in CAMPS[camp_group].items():
            writer.writerow([key])

            value = sorted(value, key=lambda d: sort_shirts_by_size(d["Shirt Size"]))

            for val in value:
                writer.writerow(val.values())

    with open(f"./church/roster_count_{camp_group}.csv", "w", newline="") as csvfile:
        writer = csv.writer(csvfile)

        count = dict((x, y) for x, y in church_count[camp_group].items())
        sorted_count = dict(sorted(count.items()))

        for val in sorted_count.items():
            writer.writerow(val)

        # writer.writerow(value)
