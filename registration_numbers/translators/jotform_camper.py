import json

from helpers import get_nested_value
from models.camper import Camper


def translate_camper(raw: dict) -> Camper:
    student_camp = get_nested_value(raw, "whichCampStudent", None)
    chaperone_camp = get_nested_value(raw, "whichCampChaperone", None)
    camp = student_camp if student_camp is not None else chaperone_camp

    payment_array = get_nested_value(raw, "paymentSent.paymentArray")
    payment = 0.00
    if payment_array is not None:
        payment += float(get_nested_value(json.loads(payment_array), "total", 0.00))

    registration_type_date = get_nested_value(raw, "submit_date_for_email_triggers.datetime")
    if registration_type_date is None:
        registration_type_date = get_nested_value(raw, "submission_date")

    return Camper(
        submission_id=get_nested_value(raw, "id"),
        submission_date=get_nested_value(raw, "submission_date"),
        approval_status=get_nested_value(raw, "status"),
        registration_type=get_nested_value(raw, "registrationType"),
        registration_type_date=registration_type_date,
        camp=camp,
        first_name=get_nested_value(raw, "yourName.first"),
        last_name=get_nested_value(raw, "yourName.last"),
        birthday=get_nested_value(raw, "birthday.datetime"),
        street_address=get_nested_value(raw, "address.addr_line1"),
        street_address2=get_nested_value(raw, "address.addr_line2"),
        city=get_nested_value(raw, "address.city"),
        state=get_nested_value(raw, "address.state"),
        zip_code=get_nested_value(raw, "address.postal"),
        country=get_nested_value(raw, "address.country", "United States"),
        cell_phone=get_nested_value(raw, "cellPhone.full", ""),  # Shouldn't be none
        student_email=get_nested_value(raw, "yourEmail"),
        gender=get_nested_value(raw, "gender").lower(),
        shirt_size=get_nested_value(raw, "shirtSize"),
        medical_concerns=get_nested_value(raw, "medicalConcerns"),
        dietary_restrictions=get_nested_value(raw, "submission_id"),  # Needs Updated
        allergies=get_nested_value(raw, "generalAllergies"),
        food_allergies=get_nested_value(raw, "submission_id"),  # Needs Updated
        past_surgeries=get_nested_value(raw, "pastSurgeries"),
        medications=get_nested_value(raw, "medications"),
        guardian_first_name=get_nested_value(raw, "emergencyContactPerson.first"),
        guardian_last_name=get_nested_value(raw, "emergencyContactPerson.last"),
        guardian_home_phone=get_nested_value(raw, "emergencyHomePhone.full"),
        guardian_work_phone=get_nested_value(raw, "emergencyWorkPhone.full"),
        guardian_contact_phone=get_nested_value(raw, "emergencyContactPhone.full"),
        insurance_company=get_nested_value(raw, "insuranceCompany"),
        policy_number=get_nested_value(raw, "policyNumber"),
        church=get_nested_value(raw, "whatChurch"),
        youth_leader_first_name=get_nested_value(raw, "youthLeader.first"),
        youth_leader_last_name=get_nested_value(raw, "youthLeader.last"),
        youth_leader_email=get_nested_value(raw, "youthLeaderEmail"),
        will_youth_leader_be_present=get_nested_value(raw, "youth_leader_attending"),
        room_with=get_nested_value(raw, "join_with_church"),
        convicted_of_crime=get_nested_value(raw, "haveYou"),
        crime_details=get_nested_value(raw, "convictionExplanation"),  # Needs Updated
        under_cps_investigation=get_nested_value(raw, "areYou55"),
        cps_investigation_details=get_nested_value(raw, "areabuseExplanationYou55"),  # These need updated
        sexual_harassment=get_nested_value(raw, "haveYou57"),
        sexual_harassment_details=get_nested_value(raw, "harassmentExplanation"),  # Needs Updated
        statement_of_faith=get_nested_value(raw, "statementOf"),
        reason_for_counselor=get_nested_value(raw, "whyDo"),
        payment=payment,
    )
