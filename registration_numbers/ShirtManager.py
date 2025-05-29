from typing import Any
from config import AGE_GROUPS, CAMPS


class ShirtManager():
    def __init__(self, level_specific: bool = False, levels: dict[str: dict] = None):
        self._level_specific = level_specific
        self._levels = levels
        # This doesn't need to be here or it should be a tally of counts
        self._totals = {
            "high_school": {},
            "middle_school": {}
        }
        self._church_shirts_roster = {
            "high_school": {},
            "middle_school": {}
        }

    def create_church_entry(self, process: str, age_group: AGE_GROUPS, church: str) -> None:
        if process == "totals" and church not in self._totals[age_group]:
            self._totals[age_group][church] = 1
            # self._totals[age_group] = {**self._totals[age_group], church: 1}
        elif process == "individuals" and church not in self._church_shirts_roster[age_group]:
            self._church_shirts_roster[age_group] = {**self._church_shirts_roster[age_group], church: []}

    def add_to_church_count(self, age_group: AGE_GROUPS, church: str) -> None:
        self.create_church_entry("totals", age_group, church)
        self._totals[age_group][church] += 1

    def add_to_shirt_roster(self, name: str, shirt_size: str, age_group: AGE_GROUPS, church: str) -> None:
        self.create_church_entry("individuals", age_group, church)
        self._church_shirts_roster[age_group][church].append({
            "no": len(self._church_shirts_roster[age_group][church]) + 1,
            "Name": name,
            "Shirt Size": shirt_size,
        })

    def create_individual_entry(self, row_data: dict[str: str]) -> None:
        age_group = CAMPS.HIGH_SCHOOL
        attending_camp = row_data["Which Camp?"].lower() \
            if row_data["Which Camp?"] != "" \
            else row_data["At which camp will you be a chaperone?"].lower()
        church = row_data["What church are you a part of?"]
        name = f"{row_data['First Name']} {row_data['Last Name']}"

        if attending_camp == CAMPS.HIGH_SCHOOL:
            age_group = AGE_GROUPS.HIGH_SCHOOL
            self.add_to_church_count(age_group, church)
            self.add_to_shirt_roster(name, row_data["Shirt Size"], age_group, church)

        elif attending_camp == CAMPS.MIDDLE_SCHOOL:
            age_group = AGE_GROUPS.MIDDLE_SCHOOL
            self.add_to_church_count(age_group, church)
            self.add_to_shirt_roster(name, row_data["Shirt Size"], age_group, church)

        else:
            self.add_to_church_count(AGE_GROUPS.MIDDLE_SCHOOL, church)
            self.add_to_shirt_roster(name, row_data["Shirt Size"], AGE_GROUPS.MIDDLE_SCHOOL, church)
            
            self.add_to_church_count(AGE_GROUPS.HIGH_SCHOOL, church)
            self.add_to_shirt_roster(name, row_data["Shirt Size"], AGE_GROUPS.HIGH_SCHOOL, church)

    @property
    def get_shirt_master(self) -> dict[str: Any]:
        return self._totals

    @property
    def get_shirt_roster(self) -> dict[str: Any]:
        return self._church_shirts_roster
