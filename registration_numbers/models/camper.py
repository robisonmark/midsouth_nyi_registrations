from typing import Union
from pydantic import BaseModel, EmailStr  # type: ignore
from datetime import datetime

from enums import Camp, Gender, ShirtSize, RegistrationType


class Camper(BaseModel):
    submission_id: str
    submission_date: datetime
    approval_status: str
    registration_type: RegistrationType
    camp: Camp
    first_name: str
    last_name: str
    birthday: datetime
    street_address: str
    street_address2: Union[str, None] = None
    city: str
    state: str
    zip_code: str
    cell_phone: Union[str, None] = None
    student_email: EmailStr
    gender: Gender
    shirt_size: ShirtSize
    medical_conditions: Union[str, None] = None
    dietary_restrictions: Union[str, None] = None
    allergies: Union[str, None] = None
    food_allergies: Union[str, None] = None
    past_surgeries: Union[str, None] = None
    medications: Union[str, None] = None
    guardian_first_name: Union[str, None] = None  # Should these be nullable or only added for student
    guardian_last_name: Union[str, None] = None  # Should these be nullable or only added for student
    guardian_home_phone: Union[str, None] = None  # Should these be nullable or only added for student
    guardian_work_phone: Union[str, None] = None  # Should these be nullable or only added for student
    guardian_contact_phone: Union[str, None] = None  # Should these be nullable or only added for student
    insurance_company: Union[str, None] = None  # Should these be nullable or only added for student
    policy_number: Union[str, None] = None  # Should these be nullable or only added for student
    church: Union[str, None] = None
    youth_leader_first_name: Union[str, None] = None
    youth_leader_last_name: Union[str, None] = None
    youth_leader_email: Union[EmailStr, None] = None
    will_youth_leader_be_present: Union[bool, None] = None
    room_with: Union[str, None] = None
    convicted_of_crime: Union[bool, None] = None
    crime_details: Union[str, None] = None
    under_cps_investigation: Union[bool, None] = None
    cps_investigation_details: Union[str, None] = None
    sexual_harassment: Union[bool, None] = None
    sexual_harassment_details: Union[str, None] = None
    statement_of_faith: Union[str, None] = None
    reason_for_counselor: Union[str, None] = None
    payment: float
