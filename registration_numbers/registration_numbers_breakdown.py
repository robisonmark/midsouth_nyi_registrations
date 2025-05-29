import os
import datetime

import xlrd2
import xlsxwriter

LAST_NAME_COLUMN = 7
MAIN_ROSTER = []

fields = [
    'Date',
    'Are you a student, chaperone, staff',
    'Which Camp?',
    'At which camp will you be a chaperone?',
    'What church are you a part of?',
    'Payment',
    'Camp',
    'Price',
    'Paid',
    'Total Due'
]


file_path = "./church/2025-05-24/"

standard_data_points = {
    "early": 0,
    "standard": 0,
    "late": 0,
    "total_students": 0,
    "total_chaperones": 0,
    "total_staff": 0,
    "total": 0
}
registration_numbers = {
    "early": {
        "high school camp": 0,
        "middle school camp": 0
    },
    "standard": {
        "high school camp": 0,
        "middle school camp": 0
    },
    "late": {
        "high school camp": 0,
        "middle school camp": 0
    },
    "total_students": {
        "high school camp": 0,
        "middle school camp": 0
    },
    "total_chaperones": {
        "high school camp": 0,
        "middle school camp": 0,
        "both camps": 0,
    },
    "total_staff": {
        "high school camp": 0,
        "middle school camp": 0,
        "both camps": 0,
    },
    "total": 0
}


def create_column_map(excel_columns: list = [], columns_to_map: list = []):
    if len(excel_columns) == 0 or len(columns_to_map) == 0:
        raise Exception('Please provide columns to map')

    column_map = {}
    for column in columns_to_map:
        column_index = excel_columns.index(column)
        column_map[column] = column_index

    return column_map


for root, dirs, files in os.walk(file_path, topdown=False):
    for name in files:
        if name == ".DS_Store":
            continue
        workbook_file = os.path.join(root, name)
        book = xlrd2.open_workbook(workbook_file)
        print("The number of worksheets is {0}".format(book.nsheets))
        print("Worksheet name(s): {0}".format(book.sheet_names()))
        sh = book.sheet_by_index(0)

        column_names = sh.row_values(rowx=0)
        column_map = create_column_map(column_names, fields)

        for rx in range(sh.nrows - 1):
            if sh.cell_value(rowx=rx + 1, colx=LAST_NAME_COLUMN) == "":
                continue

            camp = sh.cell_value(rowx=rx + 1, colx=column_map.get('Camp')).lower()
            registration_numbers["total"] += 1
            student_chaperone_selection = sh.cell_value(rowx=rx + 1,
                                                        colx=column_map.get('Are you a student, chaperone, staff'))
            if (student_chaperone_selection == 'Chaperone'):
                registration_numbers["total_chaperones"][camp] += 1
            elif (student_chaperone_selection == 'Staff'):
                registration_numbers["total_staff"][camp] += 1
            else:
                registration_numbers["total_students"][camp] += 1
                registration_date = datetime.datetime.strptime(sh.cell_value(rowx=rx + 1,
                                                                             colx=column_map.get('Date')), "%b %d, %Y")

                if registration_date <= datetime.datetime(2025, 5, 2):
                    registration_numbers["early"][camp] += 1
                elif registration_date > datetime.datetime(2025, 5, 2) \
                        and registration_date <= datetime.datetime(2025, 5, 16):
                    registration_numbers["standard"][camp] += 1
                elif registration_date > datetime.datetime(2025, 5, 16):
                    registration_numbers["late"][camp] += 1

        book.release_resources()


class RegistrationNumbers:
    def create_excel(self, file_dir: str = "./registration_numbers.csv"):
        workbook = xlsxwriter.Workbook('./registration_numbers.xlsx')
        worksheet = workbook.add_worksheet('Registration Breakdown')

        head = [
            'Camp',
            *list(standard_data_points.keys())
        ]

        bold = workbook.add_format({'bold': True})

        # # Make some of the columns wider for clarity.
        worksheet.set_column(0, 1, 20)

        row_titles = [*list(registration_numbers["total_chaperones"].keys()), "total"]

        # # Write the data.
        worksheet.write_row(0, 0, head, bold)
        worksheet.write_column(1, 0, row_titles)
        worksheet.write_column(1, 1, registration_numbers["early"].values())
        worksheet.write_column(1, 2, registration_numbers["standard"].values())
        worksheet.write_column(1, 3, registration_numbers["late"].values())
        worksheet.write_column(1, 4, registration_numbers["total_students"].values())
        worksheet.write_column(1, 5, registration_numbers["total_chaperones"].values())

        worksheet.write_formula(1, 6, '=SUM(E2, F2)', bold)
        worksheet.write_formula(2, 6, '=SUM(E3, F3)', bold)
        worksheet.write_formula(3, 6, '=SUM(E4, F4)', bold)
        worksheet.write_formula(4, 1, '=SUM(B2:B4)', bold)
        worksheet.write_formula(4, 2, '=SUM(C2:C4)', bold)
        worksheet.write_formula(4, 3, '=SUM(D2:D4)', bold)
        worksheet.write_formula(4, 4, '=SUM(E2:E4)', bold)
        worksheet.write_formula(4, 5, '=SUM(F2:F4)', bold)
        worksheet.write(4, 6, registration_numbers['total'], bold)

        workbook.close()


if __name__ == "__main__":
    rn = RegistrationNumbers()
    rn.create_excel()
