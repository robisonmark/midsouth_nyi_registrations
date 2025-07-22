import json

from helpers import get_nested_value
from models.momentum import MomentumRegistrant


def translate_registrant(registrant_data: dict) -> MomentumRegistrant:
    payment_array = get_nested_value(registrant_data, "paymentSent.paymentArray")
    payment = 0.00
    if payment_array is not None:
        payment += float(get_nested_value(json.loads(payment_array), "total", 0.00))

    registration_type_date = get_nested_value(registrant_data, "form_date.datetime")
    if registration_type_date is None:
        registration_type_date = get_nested_value(registrant_data, "submission_date")

    return MomentumRegistrant(
        submission_id=get_nested_value(registrant_data, "id"),
        submission_date=get_nested_value(registrant_data, "submission_date"),
        approval_status=get_nested_value(registrant_data, "status"),
        registration_type=get_nested_value(registrant_data, "registration_type"),
        registration_type_date=registration_type_date,  # Update eventually
        participation_status=get_nested_value(registrant_data, "participant_spectator"),
        grade_level=get_nested_value(registrant_data, "grade_level"),  # This should be a calculated field based on birthday
        age_range_talent=get_nested_value(
            registrant_data, "age_level"
        ),  # This should be a calculated field based on birthday
        age_range_individual=get_nested_value(
            registrant_data, "age_individual_sports"
        ),  # This should be a calculated field based on birthday
        first_name=get_nested_value(registrant_data, "name.first"),
        last_name=get_nested_value(registrant_data, "name.last"),
        birthday=get_nested_value(registrant_data, "birthday.datetime"),
        cell_phone=get_nested_value(registrant_data, "student_cell_phone.full", ""),
        student_email=get_nested_value(registrant_data, "student_email"),
        street_address=get_nested_value(registrant_data, "address.addr_line1"),
        street_address2=get_nested_value(registrant_data, "address.addr_line2"),
        city=get_nested_value(registrant_data, "address.city"),
        state=get_nested_value(registrant_data, "address.state"),
        zip_code=get_nested_value(registrant_data, "address.postal"),
        country=get_nested_value(registrant_data, "address.country", "United States"),
        gender=get_nested_value(registrant_data, "gender").lower(),
        shirt_size=get_nested_value(registrant_data, "shirtSize"),
        medical_concerns=get_nested_value(registrant_data, "medical_problems"),
        allergies=get_nested_value(registrant_data, "allergies"),
        past_surgeries=get_nested_value(registrant_data, "past_surgeries"),
        medications=get_nested_value(registrant_data, "medications"),
        # Is this Block Needed?
        contact_first_name=get_nested_value(registrant_data, "contact_name.first"),
        contact_last_name=get_nested_value(registrant_data, "contact_name.last"),
        contact_home_phone=get_nested_value(registrant_data, "contact_home_phone.full"),
        contact_cell_phone=get_nested_value(registrant_data, "contact_cell_phone.full"),
        insurance_company=get_nested_value(registrant_data, "insurance"),
        policy_number=get_nested_value(registrant_data, "ins_policy_number"),
        guardian_first_name=get_nested_value(registrant_data, "guardian_name.first"),
        guardian_last_name=get_nested_value(registrant_data, "guardian_name.last"),
        guardian_home_phone=get_nested_value(registrant_data, "guardian_home_phone.full"),
        guardian_work_phone=get_nested_value(registrant_data, "guardian_work_phone.full"),
        guardian_contact_phone=get_nested_value(registrant_data, "guardian_contact_phone.full"),
        church=get_nested_value(registrant_data, "church_affiliation"),
        youth_leader_first_name=get_nested_value(registrant_data, "youth_leader_name.first"),
        youth_leader_last_name=get_nested_value(registrant_data, "youth_leader_name.last"),
        youth_leader_email=get_nested_value(registrant_data, "youth_leader_email"),
        attending_tnt=get_nested_value(
            registrant_data, "attending_tnt", False
        ),  # Should this be nullable or only added for student
        academic_events=get_nested_value(registrant_data, "academic_events", []),
        art_events=get_nested_value(registrant_data, "art_events", []),
        creative_ministries_events=get_nested_value(registrant_data, "creative_ministries_events", []),
        music_events=get_nested_value(registrant_data, "music_events", []),
        quizzing_events=get_nested_value(registrant_data, "quizzing_events", []),
        individual_sports_events=get_nested_value(registrant_data, "individual_sports_events", []),
        team_sports_events=get_nested_value(registrant_data, "team_sports_events", []),
        event_errors=get_nested_value(registrant_data, "event_errors", []),  # List of errors for events if any
        payment=payment,
    )
