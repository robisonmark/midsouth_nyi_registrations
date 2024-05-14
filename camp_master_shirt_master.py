import xlrd2
import os
import csv

import datetime
hs_output_dict = {}
ms_output_dict = {}
file_path = './church/2023-06-17/'
for root, dirs, files in os.walk(file_path, topdown=False):
   for name in files:
      print(os.path.join(root, name))
      if name == '.DS_Store':
         continue
      workbook_file = os.path.join(root, name)
      book = xlrd2.open_workbook(workbook_file)
      print("The number of worksheets is {0}".format(book.nsheets))
      print("Worksheet name(s): {0}".format(book.sheet_names()))
      sh = book.sheet_by_index(0)
      #   print("{0} {1} {2}".format(sh.name, sh.nrows, sh.ncols))
      #   print("Cell D30 is {0}".format(sh.cell_value(rowx=2, colx=3)))
      for rx in range(sh.nrows-1):
        if sh.cell_value(rowx=rx+1, colx=5) == '':
           continue
        given_name = sh.cell_value(rowx=rx+1, colx=4)
        family_name = sh.cell_value(rowx=rx+1, colx=5)
        name = f'{family_name}, {given_name}'
        church =  sh.cell_value(rowx=rx+1, colx=6)
        shirt =  sh.cell_value(rowx=rx+1, colx=9)
        mr = ''
        tower = ''
        tf = 1 if sh.cell_value(rowx=rx+1, colx=8) == 'Female' and sh.cell_value(rowx=rx+1, colx=7) == 'Student' else ''
        tm = 1 if sh.cell_value(rowx=rx+1, colx=8) == 'Male' and sh.cell_value(rowx=rx+1, colx=7) == 'Student' else ''
        af = 1 if sh.cell_value(rowx=rx+1, colx=8) == 'Female' and sh.cell_value(rowx=rx+1, colx=7) == 'Chaperone' else ''
        am = 1 if sh.cell_value(rowx=rx+1, colx=8) == 'Male' and sh.cell_value(rowx=rx+1, colx=7) == 'Chaperone' else ''
        datestr =  sh.cell_value(rowx=rx+1, colx=12)
        print(datestr)
        date_parse = datetime.datetime.strptime(datestr, '%b %d, %Y')
        print(date_parse)
        first_date = 1 if sh.cell_value(rowx=rx+1, colx=7) != 'Chaperone' and date_parse <= datetime.datetime(2023, 5, 15) else ''
        second_date = 1 if sh.cell_value(rowx=rx+1, colx=7) != 'Chaperone' and date_parse > datetime.datetime(2023, 5, 15) and date_parse <= datetime.datetime(2023, 6, 1) else ''
        late_date = 1 if sh.cell_value(rowx=rx+1, colx=7) != 'Chaperone' and date_parse > datetime.datetime(2023, 6, 1) else ''
        # late_fee = 250 if date_parse > datetime.datetime(2023, 6, 1) else ''
        adult_one = 1 if sh.cell_value(rowx=rx+1, colx=7) == 'Chaperone' and sh.cell_value(rowx=rx+1, colx=56).lower() != 'both camps' else ''
        adult_both = 1 if sh.cell_value(rowx=rx+1, colx=7) == 'Chaperone' and sh.cell_value(rowx=rx+1, colx=56).lower() == 'both camps' else ''
        pay_form = 'online' if sh.cell_value(rowx=rx+1, colx=10) != '' else ''
        # print(sh.cell_value(rowx=rx+1, colx=6).lower())
        if sh.cell_value(rowx=rx+1, colx=56).lower() == 'high school camp':
            if church not in hs_output_dict:
                print(church)
                hs_output_dict[church] = []
            hs_output_dict[church].append(
                {
                    'no': len(hs_output_dict[church]),
                    'Name': name,
                    'Shirt Size': shirt
                }
            )
        elif sh.cell_value(rowx=rx+1, colx=56).lower() == 'both camps':
            if church not in hs_output_dict:
                print(church)
                hs_output_dict[church] = []
            hs_output_dict[church].append( {
                    'no': len(hs_output_dict[church]),
                    'Name': name,
                    'Shirt Size': shirt
                })
        else:
            if church not in ms_output_dict:
                ms_output_dict[church] = []
            ms_output_dict[church].append( {
                    'no': len(ms_output_dict[church]),
                    'Name': name,
                    'Shirt Size': shirt
                })


with open('./church/shirt_master.csv', 'w', newline='') as csvfile:
    writer = csv.writer(csvfile)
    for key, value in ms_output_dict.items():
       writer.writerow([key])
       for val in value:
           writer.writerow(val.values())
