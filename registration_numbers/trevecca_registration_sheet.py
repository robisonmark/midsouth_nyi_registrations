import xlrd2
import os
import csv


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
CAMP_COLUMN = 49
STREET_ADDRESS_COLUMN = 9
STREET_ADDRESS_UNIT_COLUMN = 10
CITY_COLUMN = 11
STATE_COLUMN = 12
ZIP_CODE_COLUMN = 13

MAIN_ROSTER = []


file_path = "./church/"
for root, dirs, files in os.walk(file_path, topdown=False):
    for name in files:

        if name == ".DS_Store":
            continue
        workbook_file = os.path.join(root, name)
        book = xlrd2.open_workbook(workbook_file)
        print("The number of worksheets is {0}".format(book.nsheets))
        print("Worksheet name(s): {0}".format(book.sheet_names()))
        sh = book.sheet_by_index(0)

        for rx in range(sh.nrows - 1):
            if sh.cell_value(rowx=rx + 1, colx=LAST_NAME_COLUMN) == "":
                continue
            given_name = sh.cell_value(rowx=rx + 1, colx=FIRST_NAME_COLUMN)
            family_name = sh.cell_value(rowx=rx + 1, colx=LAST_NAME_COLUMN)
            name = f"{family_name}, {given_name}"
            church = sh.cell_value(rowx=rx + 1, colx=CHURCH_COLUMN)

            student_chaperone_selection = sh.cell_value(rowx=rx + 1, colx=STUDENT_CHAPERONE_COLUMN)

            camp = sh.cell_value(rowx=rx + 1, colx=CAMP_COLUMN).lower()

            street_address = sh.cell_value(rowx=rx + 1, colx=STREET_ADDRESS_COLUMN)
            street_address_unit = sh.cell_value(rowx=rx + 1, colx=STREET_ADDRESS_UNIT_COLUMN)
            city = sh.cell_value(rowx=rx + 1, colx=CITY_COLUMN)
            state = sh.cell_value(rowx=rx + 1, colx=STATE_COLUMN)
            zip_code = sh.cell_value(rowx=rx + 1, colx=ZIP_CODE_COLUMN)

            address_string = f"{street_address} {street_address_unit}, {city}, {state} {zip_code}"

            if student_chaperone_selection != "Chaperone":
                MAIN_ROSTER.append(
                        {
                            "no": len(MAIN_ROSTER) + 1,
                            "Church": church,
                            "Name": name,
                            "Camp": camp,
                            "Address": address_string,

                        }
                    )

class Trevecca:
    def create_list(self, file_dir: str = "./church/trevecca_roster.csv"):
        with open(file_dir, "w", newline="") as csvfile:
                writer = csv.writer(csvfile)
                writer.writerow(MAIN_ROSTER[0].keys())
                for student in MAIN_ROSTER:
                    writer.writerow(student.values())


