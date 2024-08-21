from cement import shell

from trevecca_registration_sheet import Trevecca
from registation_numbers_breakdown import RegistrationNumbers

if __name__ == "__main__":
    process = shell.Prompt("Which process do you want to run?",
                 options=[
                     'Church Count Sheet',
                     'General Camp Roster',
                     'Shirt Lists - By Church',
                     'Registration Numbers Breakdown',
                     'Trevecca Student List',
                 ],
                 numbered = True)

    res = process.prompt()

    if res == "Trevecca Student List":
        tnu = Trevecca()
        tnu.create_list()
    
    if res == "Registration Numbers Breakdown":
        reg_report = RegistrationNumbers()
        reg_report.create_excel()

