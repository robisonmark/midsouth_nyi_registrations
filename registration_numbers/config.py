class EVENT:
    CAMP = "CAMP"
    MOMENTUM = "MOMENTUM"
    TNT = "TNT"


class AGE_GROUPS:
    HIGH_SCHOOL = "high_school"
    MIDDLE_SCHOOL = "middle_school"


class CAMPS:
    HIGH_SCHOOL = "high school camp"
    MIDDLE_SCHOOL = "middle school camp"
    BOTH = "both camps"


class INDIVIDUAL_SPORTS_AGE_GROUPS:
    pass


class ARTS_AGE_GROUPS:
    pass


API_KEY = "ca959f5b2862927b9e91bd7b69675e76"  # DO NOT COMMIT
API_URL = "https://api.jotform.com/form/{formID}/submissions?apiKey={apiKey}"

CAMP_FORM_ID = "250758652718164"
MOMENTUM_FORM_ID = "251575366309160"

# class Configuration():
#     def church_list():
#         # gather from csv file
#         return []

#     def exclude_from_write(event: str) -> list[str] | None:
#         if event == EVENT.CAMP:
#             pass

#         return [
#           "First Name",
#           "Last Name",
#           "Grade Level",
#           "Age Level Individual Sport",
#           "Age Level",
#           "What church are you a part of?"]
