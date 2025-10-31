import json
import re

from enums import (
    AgeRangeIndividualSports,
    AgeRangeTalent,
    EventsAcademic,
    EventsArt,
    EventsCreativeMinistries,
    EventsIndividualSports,
    EventsMusic,
    EventsQuizzing,
    EventsTeamSports,
    Gender,
    ParticipationStatus,
    RegistrationType,
    ShirtSize,
)
from helpers import get_nested_value
from models.momentum import MomentumRegistrant


def translate_registrant(registrant_data: dict) -> MomentumRegistrant:
    payment_array = get_nested_value(registrant_data, "payment_amount.paymentArray")
    payment = 0.00
    if payment_array is not None:
        payment += float(get_nested_value(json.loads(payment_array), "total", 0.00))

    registration_type_date = get_nested_value(registrant_data, "form_date.datetime")
    if registration_type_date is None:
        registration_type_date = get_nested_value(registrant_data, "submission_date")

    participation_status = get_nested_value(registrant_data, "participant_spectator", "")
    if participation_status is not None:
        participation_status = participation_status.split("(")[0].strip()

    quizzing_events = get_nested_value(registrant_data, "quizzing")
    quizzing = []
    if quizzing_events != "Neither" and quizzing_events is not None:
        quizzing.append(EventsQuizzing(re.sub(r"[*]", "", quizzing_events).strip()))

    print(f"Quizzing Events: {quizzing}")

    academics = [
        EventsAcademic(re.sub(r"[*]", "", academic_event).strip())
        for academic_event in get_nested_value(registrant_data, "academics", [])
    ]

    arts = [EventsArt(re.sub(r"[*]", "", art_event).strip()) for art_event in get_nested_value(registrant_data, "art", [])]
    academics = [
        EventsAcademic(re.sub(r"[*]", "", academic_event).strip())
        for academic_event in get_nested_value(registrant_data, "academics", [])
    ]

    creative_ministries = [
        EventsCreativeMinistries(re.sub(r"[*]", "", creative_event).strip())
        for creative_event in [
            *get_nested_value(registrant_data, "creative_ministries", []),
            *get_nested_value(registrant_data, "creative_writing", []),
            *get_nested_value(registrant_data, "speech", []),
        ]
    ]

    music = [
        EventsMusic(re.sub(r"[*]", "", creative_event).strip())
        for creative_event in [
            *get_nested_value(registrant_data, "vocal_music", []),
            *get_nested_value(registrant_data, "instrumental_music", []),
        ]
    ]

    individual_sports = [
        EventsIndividualSports(re.sub(r"[*]", "", indv_sport).strip())
        for indv_sport in get_nested_value(registrant_data, "individual_sports", [])
    ]

    team_sports = [
        EventsTeamSports(re.sub(r"[*]", "", team_sport).strip())
        for team_sport in get_nested_value(registrant_data, "team_sports", [])
    ]

    return MomentumRegistrant(
        submission_id=get_nested_value(registrant_data, "id"),
        submission_date=get_nested_value(registrant_data, "submission_date"),
        approval_status=get_nested_value(registrant_data, "status"),
        registration_type=get_nested_value(registrant_data, "registration_type"),
        registration_type_date=registration_type_date,  # Update eventually
        participation_status=participation_status,
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
        academic_events=academics,  # This probably needs to be split in the model
        art_events=arts,
        creative_ministries_events=creative_ministries,
        music_events=music,
        quizzing_events=quizzing,
        individual_sports_events=individual_sports,
        team_sports_events=team_sports,
        event_errors=get_nested_value(registrant_data, "event_errors", []),  # List of errors for events if any
        payment=payment,
    )
