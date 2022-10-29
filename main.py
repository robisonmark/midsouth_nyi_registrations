# To Do: read in csv
# get break apart data by categories
# break down by age
import csv
import pandas as pd

event_exclusion_data_cols = [
    'Submission Date',
    'Form Submission Date',
    'I am registering as a',
    'Are you Participating or Spectating?',
    'District',
    'Birthday',
    'Street Address',
    'Street Address Line 2',
    'City',
    'State / Province',
    'Postal / Zip Code',
    'Cell Phone',
    'Your Email',
    'Gender',
    'Shirt Size',
    'First Name.1',
    'Last Name.1',
    'What is your youth leader/pastor\'s email?',
    'First Name.2',
    'Last Name.2',
    'Parent/Guardian Email',
    'Parent Cell Phone',
    'Please list any medical problems',
    'Please list any allergies',
    'Please list past surgeries',
    'Please list any medications and dosage you will be taking',
    'First Name.3',
    'Last Name.3',
    'Home Phone',
    'Work Phone',
    'Contact Phone',
    'Insurance Company',
    'Policy Number #',
    'Do you plan on attending TNT@TNU April 13th-April 16th?',
    'Payment'
]

write_file_exlusion = ['First Name', 'Last Name', 'Grade Level', 'Age Level Individual Sport', 'Age Level', 'What church are you a part of?']
multiple_possible = [
    'Art',
    'Creative Ministries',
    'Creative Writing',
    'Speech',
    'Academics',
    'Vocal Music',
    'Instrumental Music',
    'Individual Sports',
    'Team Sports',
    'Quizzing'
]


def write_csv(df, df_name, filename):
    if df_name not in write_file_exlusion:
        df.to_csv(f'./{filename}.csv', index=False,)

def create_student_categories():
    df_original = pd.read_csv('./momentum_numbers.csv')

    cols = df_original.columns;
    
    category_cols = [];
    for col in cols:
        if col not in event_exclusion_data_cols:
            category_cols.append(col)
    
    for category_col in category_cols:
        loc_cols = ['First Name', 'Last Name', 'Grade Level', 'What church are you a part of?']
        group_by_cols = [category_col]
        if 'sport' in category_col.lower():
           loc_cols.append('Age Level Individual Sport')
        else:
           loc_cols.append('Age Level')

        if category_col == 'Individual Sports':
           group_by_cols.append('Age Level Individual Sport')
        if category_col == 'Team Sports':
           group_by_cols.append('What church are you a part of?')
        if 'Music' in category_col:
            group_by_cols.append('What church are you a part of?')
        
        loc_cols.append(category_col)

        df = df_original.copy(deep=True)
        df = df.loc[:, loc_cols].dropna(subset=[category_col])

        if category_col in multiple_possible:
            # df = pd.pivot(df, [category_col], loc_cols)
            df = df.set_index(category_col).unstack()
            # df = pd.pivot_table(df, index=category_col, values='First Name', columns=loc_cols)
            print(df)

        filename = category_col.replace(' ', '_').lower()
        
        write_csv(df, category_col, filename)




if __name__ == "__main__":
    

    church_data_cols = [
        'First Name',
        'Last Name',
        'Form Submission Date',
        'I am registering as a',
        'Are you Participating or Spectating?',
        'Grade Level',
        'District',
        'Age Level',
        'Age Level Individual Sport',
        'Birthday',
        'Street Address',
        'Street Address Line 2',
        'City',
        'State / Province',
        'Postal / Zip Code',
        'Cell Phone',
        'Your Email',
        'Gender',
        'Shirt Size',
        'What church are you a part of?',
        'First Name.1',
        'Last Name.1',
        'What is your youth leader/pastor\'s email?',
        'First Name.2',
        'Last Name.2',
        'Parent/Guardian Email',
        'Parent Cell Phone',
        'Please list any medical problems',
        'Please list any allergies',
        'Please list past surgeries',
        'Please list any medications and dosage you will be taking',
        'First Name.3',
        'Last Name.3',
        'Home Phone',
        'Work Phone',
        'Contact Phone',
        'Insurance Company',
        'Policy Number #',
        'Do you plan on attending TNT@TNU April 13th-April 16th?',
        'Payment'
    ]

    create_student_categories();
