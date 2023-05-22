pricing_breakdown = {
    'students': {
        '05-17-23': 175,
        '06-01-23': 225,
    },
    'chaperones': {
        'single_camp': 125,
        'both_camps': 200,
    }
}

def write_csv(df, df_name, process, filename):
    create_directory(process)
    
    df.to_csv(f'{filename}.csv')