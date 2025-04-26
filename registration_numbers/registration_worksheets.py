# TODO: Get List of Churches
# TODO: Create a Pricing Breakdown for each event to pull in
# TODO: Get a file walker
# TODO: set up linting/pylance
import os

from datetime import date

import shutil

from config import EVENT, AGE_GROUP, Configuration

DEFAULT_FILE_ROOT = "./"


class FileRunner():
    def __init__(self, event: EVENT, age_group: AGE_GROUP):
        self.event = event
        self.age_group = age_group

    def gather_files(self, file_root: str = DEFAULT_FILE_ROOT) -> list[str]:
        files = os.listdir(file_root)
        files = [f for f in files if os.path.isfile(f'{file_root}/{f}') and f != '.DS_Store'] 
        
        return files


    def create_directory(self, process, file_root: str = DEFAULT_FILE_ROOT) -> None:
        outdir = f"{file_root}/output/{self.event}/{process}/{date.today()}/"
        
        if not os.path.exists(outdir):
            os.mkdir(outdir)
    

    # Remove Dataframe for lightweight package
    def write_to_csv(self, df, df_name, process, filename: str) -> None:
        self.create_directory(process)
        df.to_csv(f'{date.today()}_{filename}.csv')


    # Remove Dataframe for lightweight package
    def write_to_excel(self, df, df_name, process, filename, indexed=False) -> None:
        self.create_directory(process)
        if df_name not in Configuration.exclude_from_write:
            df.to_excel(f'{filename}.xlsx', index=indexed, engine='xlsxwriter')

    
    def move_file_to_complete(self, filename: str, file_root: str = DEFAULT_FILE_ROOT) -> None:
        src_file = f'{file_root}/{filename}'
        dst_file = f'./{self.event}/processed/{filename}'

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

    
