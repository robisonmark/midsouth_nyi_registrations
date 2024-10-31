import requests
import csv
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# https://poirot.auth0.com/authorize?client_id=DljJ1JWdWTIMKverkYePDW2zlGhsuDsz&response_type=token&redirect_uri=https%3A%2F%2Fbkyle.space%2Ftnt%2Flogin%2Fcallback&scope=openid%20profile%20email&state=ASbB-HjOz_Dw1Koj86Ove1mFR3U7lHft&auth0Client=eyJuYW1lIjoiYXV0aDAuanMiLCJ2ZXJzaW9uIjoiOS4yMC4wIn0%3D
"client_id": "DljJ1JWdWTIMKverkYePDW2zlGhsuDsz" # type: ignore
"response_type": "token" # type: ignore
"redirect_uri": "https://bkyle.space/tnt/login/callback" # type: ignore
"scope": "openid profile email" # type: ignore
"state": "ASbB-HjOz_Dw1Koj86Ove1mFR3U7lHft" # type: ignore
"auth0Client": "eyJuYW1lIjoiYXV0aDAuanMiLCJ2ZXJzaW9uIjoiOS4yMC4wIn0=" # type: ignore

"state": "hKFo2SB1UEJ3SEFBNTlodFc0c0tEdUxvRTB2RUJyWlI0YXVXUqFupWxvZ2luo3RpZNkgRW5JWndrOFVRU2NpT3NkNTFmbjBhQXZacm1idzhCS2ijY2lk2SBEbGpKMUpXZFdUSU1LdmVya1llUERXMnpsR2hzdURzeg"
"client": "DljJ1JWdWTIMKverkYePDW2zlGhsuDsz"
"protocol": "oauth2"
"response_type": "token"
"redirect_uri": "https://bkyle.space/tnt/login/callback"
"scope": "openid profile email"
"auth0Client": "eyJuYW1lIjoiYXV0aDAuanMiLCJ2ZXJzaW9uIjoiOS4yMC4wIn0="

AUTH0_CLIENT_ID = os.getenv('AUTH0_CLIENT_ID')
AUTH0_CLIENT_SECRET = os.getenv('AUTH0_CLIENT_SECRET')
AUTH0_DOMAIN = os.getenv('AUTH0_DOMAIN')
AUTH0_AUDIENCE = os.getenv('AUTH0_AUDIENCE')
API_URL = os.getenv('API_URL')

# GET registrants https://bkyle.space/tnt/api/registrants

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
    """Load data from a CSV file into a list of dictionaries."""
    students = []
    with open(file_path, mode='r') as file:
        reader = csv.DictReader(file)
        for row in reader:
            students.append(row)
    return students

def validate_categories(categories):
    """Ensure a student participates in 3 or fewer top-level categories."""
    return len(categories.split(',')) <= 3

def validate_gender_and_age(student):
    """Validate gender and age-specific participation rules."""
    gender = student.get('Gender', '').strip().lower()
    categories = student.get('Categories', '').lower()

    # Example gender-based rule: Males cannot play in 'ballet'
    if gender == 'male' and 'ballet' in categories:
        return False
    return True

def validate_data(students):
    """Separate valid and invalid student data."""
    valid_data = []
    invalid_data = []

    for student in students:
        if (validate_categories(student['Categories']) and 
            validate_gender_and_age(student)):
            valid_data.append(student)
        else:
            invalid_data.append(student)

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
    students = load_csv(file_path)

    # Validate data
    valid_data, invalid_data = validate_data(students)

    print(f"Valid records: {len(valid_data)}")
    print(f"Invalid records: {len(invalid_data)}")

    # Upload valid data to the API
    if valid_data:
        upload_data(API_URL, token, valid_data)

if __name__ == "__main__":
    main()
