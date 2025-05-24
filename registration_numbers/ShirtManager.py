from config import AGE_GROUPS, CAMPS

    
class ShirtManager():
    def __init__(self, level_specific: bool = False, levels: dict[str: dict] = None):
        self._level_specific = level_specific
        self._levels = levels
        self._count = {
            'high_school': {},
            'middle_school': {}
        }
    
    def create_church_entry(self, age_group: AGE_GROUPS, church: str):
        if church not in self._count[age_group]:
            self._count[age_group][church] = []

    def add_to_church_count(self, age_group: AGE_GROUPS, church: str):
        if church not in self._count[age_group]:
            self._count[age_group][church] = 1
        else:
            self._count[age_group][church] += self._count

    def create_individual_entry(self, row_data: dict[str: str]):
        age_group = CAMPS.HIGH_SCHOOL
        attending_camp = row_data["Which Camp?"].lower() \
            if row_data["Which Camp?"] != "" \
            else row_data["At which camp will you be a chaperone?"].lower()
        church = row_data["What church are you a part of?"]
        
        
        if attending_camp == CAMPS.HIGH_SCHOOL:
            age_group = AGE_GROUPS.HIGH_SCHOOL
            self.create_church_entry(age_group, church)
        elif attending_camp == CAMPS.MIDDLE_SCHOOL:
            age_group = AGE_GROUPS.MIDDLE_SCHOOL
            self.create_church_entry(age_group, church)
        else:
            self.create_church_entry(AGE_GROUPS.MIDDLE_SCHOOL, church)
            self.create_church_entry(AGE_GROUPS.HIGH_SCHOOL, church)

    @property
    def get_shirt_master(self):
        return self._count
