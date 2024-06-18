import csv
import sys
=
from cement import shell

import camp_registrations



if __name__ == "__main__":
    count = 0
    event_prompt = shell.Prompt(
        "For which event are you running registration numbers?",
        options=["Camp", "Momentum"],
        numbered=True,
    )
    file_prompt = shell.Prompt(
        "For which event are you running registration numbers?",
        options=["Camp", "Momentum"],
        numbered=True,
    )

    if event_prompt == 'Camp':
        camp_registrations()
