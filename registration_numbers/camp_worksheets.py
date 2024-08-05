from cement import shell

from trevecca_registration_sheet import Trevecca

if __name__ == "__main__":
    process = shell.Prompt("Which process do you want to run?",
                 options=[
                     'Church Count Sheet',
                     'General Camp Roster',
                     'Shirt Lists - By Church',
                     'Trevecca Student List',
                 ],
                 numbered = True)

    res = process.prompt()

    if res == "Trevecca Student List":
        Trevecca.create_list()
