from typing import OrderedDict, Any

from config import EVENT
from FileManager import FileManager
from ShirtManager import ShirtManager


class CampWorksheets():
    def __init__(self):
        self._event = EVENT.CAMP
        self.file_manager = FileManager(self._event)

    def read_file(self) -> list[OrderedDict]:
        files = self.file_manager.gather_files("./processed")
        file_data = []
        for file in files:
            if "camp" in file.lower():
                return self.file_manager.read_csv(file)

        return file_data
    
    def process_data(self, raw_data: list[dict[str: str]]) -> list[dict[str: Any]]:
        sm = ShirtManager(level_specific=True)
        for data in raw_data:
            sm.create_individual_entry(row_data=data)
        
        print(sm.get_shirt_master)


if __name__ == "__main__":
    run_camp_worksheets = CampWorksheets()
    camp_data = run_camp_worksheets.read_file()

    run_camp_worksheets.process_data(camp_data)

    # print(camp_data[0])
    # TODO: Gather Each Church Roster to export into xlsx
    # TODO: Separate Middle School and High School Camps, Making Sure to Only Charge Sponsors 1 time
    # TODO: Create Shirt Master
    # TODO: Create TNU Master
    # TODO: Create Camp Master (Start from Template - Guess Rooming)
