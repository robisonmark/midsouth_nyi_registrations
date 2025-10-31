# TODO: Get List of Churches
# TODO: Create a Pricing Breakdown for each event to pull in
# TODO: Get a file walker
# TODO: set up linting/pylance
import csv
import os
import shutil
from datetime import date
from operator import attrgetter
from typing import OrderedDict

import xlsxwriter
from config import EVENT

# PDF
from reportlab.lib.pagesizes import LETTER
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.platypus import (
    ListFlowable,
    ListItem,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
)

DEFAULT_FILE_ROOT = "."


class FileManager:
    def __init__(self, event: EVENT):
        self.event = event
        # self.age_group = age_group

    def gather_files(self, file_root: str = DEFAULT_FILE_ROOT) -> list[str]:
        files = os.listdir(file_root)
        files = [f for f in files if os.path.isfile(f"{file_root}/{f}") and f != ".DS_Store"]

        return files

    def create_directory(self, process, file_root: str = DEFAULT_FILE_ROOT, church: str = None) -> str:
        outdir = f"{file_root}/output/{self.event.lower()}/{process}/{date.today()}/"
        if church:
            outdir += f"{church}/"

        if not os.path.exists(outdir):
            os.makedirs(outdir)

        return outdir

    def read_csv(self, filename: str) -> OrderedDict:
        # change the root to an initialization property
        with open(f"./files/{filename}", newline="") as csv_file:
            # Or csv.reader(csvfile) for list rows
            reader = csv.DictReader(csv_file)
            file_data = []
            for row in reader:
                file_data.append(row)

            return file_data

    # Remove Dataframe for lightweight package

    def write_to_csv(self, df, df_name, process, filename: str) -> None:
        self.create_directory(process)
        df.to_csv(f"{date.today()}_{filename}.csv")

    def write_to_excel(self, process, filename, worksheets, church: str = None) -> None:
        """
        Writes data to an Excel file with multiple worksheets.

        Args:
            process (str): The name of the process, used to create a directory for the file.
            filename (str): The name of the Excel file to be created (without extension).
            worksheets (list): A list of dictionaries, where each dictionary represents a worksheet.
                Each dictionary should have the following structure:
                    {
                        "data": list,  # List of data entries for the worksheet.
                            Each entry in the "data" list should be a dictionary with the following keys:
                                {
                                    "type": str,  # Type of data ("header", "row", "col", or "formula").
                                    "row": int,  # Row index for the data.
                                    "col": int,  # Column index for the data.
                                    "values" or "formula": list or str,  # Data values or formula string.
                                    "format": xlsxwriter.format.Format,  # Format for the data (e.g., bold).
                                }
                    }

        Returns:
            None: The function writes the Excel file to disk and does not return any value.

        Raises:
            Any exceptions raised by xlsxwriter or file operations will propagate.
        """
        output_loc = self.create_directory(process, church=church)

        workbook = xlsxwriter.Workbook(f"{output_loc}/{filename}.xlsx")

        for worksheet_data in worksheets:
            print(f"Creating worksheet: {worksheet_data['worksheet_name']}")
            worksheet = workbook.add_worksheet(worksheet_data["worksheet_name"])
            for data in worksheet_data["data"]:
                cell_format = None
                if data["format"] is not None:
                    cell_format = workbook.add_format(data["format"])
                if data["type"] == "header":
                    worksheet.write_row(data["row"], data["col"], data["values"], cell_format)
                if data["type"] == "row":
                    worksheet.write_row(data["row"], data["col"], data["values"], cell_format)
                if data["type"] == "col":
                    worksheet.write_column(data["row"], data["col"], data["values"], cell_format)
                if data["type"] == "formula":
                    worksheet.write_column(data["row"], data["col"], data["formula"], cell_format)
                if data["type"] == "col_format":
                    worksheet.set_column(data["first_col"], data["last_col"], data["width"], cell_format)

        workbook.close()

    def write_to_txt_file(self, process, filename, content):
        """
        Writes the given content to a .txt file.

        :param file_path: Path to the .txt file.
        :param content: Content to write into the file.
        """
        output_loc = self.create_directory(process)
        with open(f"{output_loc}/{filename}.txt", "w") as file:
            file.write(content)

    def move_file_to_complete(self, filename: str, file_root: str = DEFAULT_FILE_ROOT) -> None:
        src_file = f"{file_root}/{filename}"
        dst_file = f"./{self.event}/processed/{filename}"

        shutil.move(src_file, dst_file)

    def normalize_filename(self):
        pass

    def get_column_mapping(sheet):
        header_row = sheet.row_values(0)
        column_index = {}
        for col_num, col_name in enumerate(header_row):
            column_index[col_name] = col_num

        return column_index

    def get_cell_value(sheet, row_num, column_name, column_index):
        col_num = column_index[column_name]
        return sheet.cell_value(row_num, col_num)

    def multisort(xs, specs):
        for key, reverse in reversed(specs):
            xs.sort(key=attrgetter(key), reverse=reverse)
        return xs

    def create_pdf_from_dict(self, data: dict, filename: str):
        output_loc = self.create_directory("church_summaries")

        # Create the PDF document
        doc = SimpleDocTemplate(f"{output_loc}/{filename}", pagesize=LETTER)
        elements = []

        # Define styles
        styles = getSampleStyleSheet()
        header_style = ParagraphStyle("HeaderStyle", parent=styles["Heading2"], fontName="Helvetica-Bold", spaceAfter=6)
        list_style = ParagraphStyle("ListStyle", parent=styles["Normal"], leftIndent=20, spaceAfter=4)

        # Iterate through dictionary and build PDF content
        for key, values in data.items():
            # Add section header
            elements.append(Paragraph(key, header_style))

            # Add list of items
            list_items = [ListItem(Paragraph(item, list_style)) for item in values]
            elements.append(ListFlowable(list_items, bulletType="bullet"))
            elements.append(Spacer(1, 12))  # Add space between sections

        # Build PDF
        doc.build(elements)
        print(f"✅ PDF '{filename}' created successfully.")
