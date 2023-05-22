import csv
import sys
import argparse
import pandas as pd

import camp_registrations





if __name__ == "__main__":
    parser = argparse.ArgumentParser(event = 'camp')
    parser.add_argument('--event', help = "What event are you running church numbers for")
    
    args = parser.parse_args(sys.argv[1:])
    folder = "C:\\TEMP\\"+args.folder
    event = args.event

    if event == 'camp':
        camp_registrations.constants.write_csv()
