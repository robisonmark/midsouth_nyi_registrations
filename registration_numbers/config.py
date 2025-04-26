import enum

class EVENT:
    CAMP = "CAMP"
    MOMENTUM = "MOMENTUM"
    TNT = "TNT"


class AGE_GROUP:
    HIGH_SCHOOL = "high_school"
    MIDDLE_SCHOOL = "middle_school"

class Configuration():
    def church_list():
        # gather from csv file
        return []
    
    def exclude_from_write(event: EVENT = EVENT.CAMP) -> list[str] | None:
        if event == EVENT.CAMP:
            pass

        return ["First Name", "Last Name", "Grade Level", "Age Level Individual Sport", "Age Level", "What church are you a part of?"]