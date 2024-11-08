AUTH0_KEYS = {
    "client_id": "DljJ1JWdWTIMKverkYePDW2zlGhsuDsz",
    "response_type": "token",
    "redirect_uri": "https://bkyle.space/tnt/login/callback",
    "scope": "openid profile email",
    "state": "LZGI~XMMJfirqbApp3OOPuGE1ACko1ws",
    "auth0Client": "eyJuYW1lIjoiYXV0aDAuanMiLCJ2ZXJzaW9uIjoiOS4yMC4wIn0="
}

import requests
import pandas as pd
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

AUTH0_CLIENT_ID = os.getenv('AUTH0_CLIENT_ID')
AUTH0_CLIENT_SECRET = os.getenv('AUTH0_CLIENT_SECRET')
AUTH0_DOMAIN = os.getenv('AUTH0_DOMAIN')
AUTH0_AUDIENCE = os.getenv('AUTH0_AUDIENCE')
API_URL = os.getenv('API_URL')

def get_auth0_token():
    """Authenticate with Auth0 and get an access token."""
    url = f"https://{AUTH0_DOMAIN}/oauth/token"
    headers = {'content-type': 'application/json'}
    payload = {
        'client_id': AUTH0_CLIENT_ID,
        'client_secret': AUTH0_CLIENT_SECRET,
        'audience': AUTH0_AUDIENCE,
        'grant_type': 'client_credentials'
    }

    response = requests.post(url, json=payload, headers=headers)
    response.raise_for_status()
    return response.json()['access_token']

def load_csv(file_path):
    """Load data from a CSV file into a DataFrame."""
    return pd.read_csv(file_path)

def validate_data(df):
    """Validate the data according to the rules."""
    valid_data = []
    invalid_data = []

    # Rule 1: Each student can only participate in 3 top-level categories.
    def validate_categories(categories):
        return len(categories.split(',')) <= 3

    # Rule 2: Ensure gender and age-specific participation
    def validate_gender_and_age(student, gender_column, age_column):
        # Add gender/age-specific logic here
        gender = student[gender_column]
        age = student[age_column]
        # Example logic: Ensure males can only play in 'Football', 'Basketball'
        if gender == 'Male' and 'Ballet' in student['Categories']:
            return False
        return True

    for _, row in df.iterrows():
        if validate_categories(row['Categories']) and validate_gender_and_age(row, 'Gender', 'Age'):
            valid_data.append(row.to_dict())
        else:
            invalid_data.append(row.to_dict())

    return valid_data, invalid_data

def upload_data(api_url, token, data):
    """Upload valid data to the API."""
    headers = {
        'Authorization': f'Bearer {token}',
        'Content-Type': 'application/json'
    }

    for student in data:
        response = requests.post(api_url, json=student, headers=headers)
        if response.status_code != 201:
            print(f"Failed to upload {student['Name']}: {response.text}")
        else:
            print(f"Successfully uploaded {student['Name']}")

def main():
    # Authenticate with Auth0
    token = get_auth0_token()

    # Load CSV data
    file_path = 'students.csv'
    df = load_csv(file_path)

    # Validate data
    valid_data, invalid_data = validate_data(df)

    print(f"Valid records: {len(valid_data)}")
    print(f"Invalid records: {len(invalid_data)}")

    # Upload valid data to the API
    if valid_data:
        upload_data(API_URL, token, valid_data)

if __name__ == "__main__":
    main()
