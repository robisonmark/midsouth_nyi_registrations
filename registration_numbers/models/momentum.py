from datetime import datetime
from typing import Union

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
from pydantic import (  # type: ignore
    BaseModel,
    EmailStr,
    FieldValidationInfo,
    field_validator,
    model_validator,
)


class MomentumRegistrant(BaseModel):
    submission_id: str
    submission_date: datetime
    approval_status: str
    registration_type: RegistrationType
    registration_type_date: Union[datetime, None] = None  # Can be None if not provided
    grade_level: Union[str, None] = None  # Should be a calculated field based on birthday
    age_range_talent: Union[AgeRangeTalent, None] = None  # Should be calculated based on grade level
    age_range_individual: Union[AgeRangeIndividualSports, None] = None  # Should be calculated based on birthday
    participation_status: ParticipationStatus
    first_name: str
    last_name: str
    birthday: Union[datetime, None] = None  # Can be None if not provided
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
    attending_tnt: Union[bool, None] = None  # Should this be nullable or only added for student
    payment: float
    academic_events: Union[list[EventsAcademic], None] = None  # Should this be nullable or only added for student
    art_events: Union[list[EventsArt], None] = None  # Should this be nullable or only added for student
    creative_ministries_events: Union[list[EventsCreativeMinistries], None] = (
        None  # Should this be nullable or only added for student
    )
    # math_and_business_events: Union[list[EventsAcademic.ACCOUNTING, EventsAcademic.MATH], None] = (
    #     None  # Should this be nullable or only added for student
    # )
    music_events: Union[list[EventsMusic], None] = None  # Should this be nullable or only added for student
    quizzing_events: Union[list[EventsQuizzing], None] = None  # Should this be nullable or only added for student
    individual_sports_events: Union[list[EventsIndividualSports], None] = (
        None  # Should this be nullable or only added for student
    )
    # science_events: Union[list[EventsAcademic.SCIENCE], None] = None  # Should this be nullable or only added for student
    team_sports_events: Union[list[EventsTeamSports], None] = None  # Should this be nullable or only added for student
    event_errors: Union[list[str], None] = None  # List of errors for events if any

    # @classmethod
    # def create_event_error(cls, error: str):
    #     if cls.event_errors is None:
    #         cls.event_errors = []
    #     cls.event_errors.append(error)

    @field_validator("registration_type", mode="before")
    @classmethod
    def validate_registration_type(cls, value):
        if value is not None:
            return value.lower()

    @field_validator("age_range_talent", mode="before")
    @classmethod
    def validate_age_range_talent(cls, value, info: FieldValidationInfo):
        grade_level = info.data.get("grade_level")
        if not grade_level:
            return None

        if grade_level in ["10th", "11th", "12th"]:
            return AgeRangeTalent.SENIOR_YOUTH
        elif grade_level in ["6th", "7th", "8th", "9th"]:
            return AgeRangeTalent.EARLY_YOUTH
        else:
            return None

    @field_validator("age_range_individual", mode="before")
    @classmethod
    def validate_age_range_individual_sports(cls, value, info: FieldValidationInfo):
        birthday = info.data.get("birthday")
        if not birthday:
            return None

        current_year = datetime.now().year
        cut_off_date = datetime(current_year, 9, 1)

        cut_off_delta = birthday - cut_off_date
        if cut_off_delta.years <= AgeRangeIndividualSports.JUNIOR_HIGH:
            return AgeRangeIndividualSports.JUNIOR_HIGH
        elif (
            cut_off_delta.years > AgeRangeIndividualSports.JUNIOR_HIGH
            and cut_off_delta.years <= AgeRangeIndividualSports.MIDDLE_HIGH
        ):
            return AgeRangeIndividualSports.MIDDLE_HIGH
        elif (
            cut_off_delta.years > AgeRangeIndividualSports.MIDDLE_HIGH
            and cut_off_delta.years <= AgeRangeIndividualSports.SENIOR_HIGH
        ):
            return AgeRangeIndividualSports.SENIOR_HIGH

        else:
            return None

    @field_validator("participation_status", mode="before")
    @classmethod
    def validate_participation_status(cls, value):
        if value == "Participant":
            return ParticipationStatus.PARTICIPANT
        elif value == "Spectator":
            return ParticipationStatus.SPECTATOR
        else:
            return ParticipationStatus.SPECTATOR

    @field_validator("attending_tnt", mode="before")
    @classmethod
    def validate_attending_tnt(cls, value):
        if value == "Yes":
            return True
        elif value == "Unsure":
            return True

        return False

    @model_validator(mode="after")
    @classmethod
    def validate_event_registration(cls, data: "MomentumRegistrant"):
        if data.registration_type == RegistrationType.STAFF or data.registration_type == RegistrationType.CHAPERONE:
            return data

        if data.participation_status == ParticipationStatus.SPECTATOR:
            # TODO: Create error helper function
            if data.event_errors is None:
                data.event_errors = []

            if (
                (data.academic_events is not None and len(data.academic_events) > 0)
                or (data.art_events is not None and len(data.art_events) > 0)
                or (data.creative_ministries_events is not None and len(data.creative_ministries_events) > 0)
                or (data.music_events is not None and len(data.music_events) > 0)
                or (data.quizzing_events is not None and len(data.quizzing_events) > 0)
                or (data.individual_sports_events is not None and len(data.individual_sports_events) > 0)
                or (data.team_sports_events is not None and len(data.team_sports_events) > 0)
            ):
                data.event_errors.append("Spectators cannot register for events")

            data.art_events = []
            data.creative_ministries_events = []
            # data.math_and_business_events = []
            data.music_events = []
            data.quizzing_events = []
            data.individual_sports_events = []
            data.team_sports_events = []

            return data

        if data.art_events is not None:
            # These need to consider broad categories
            pass
            # if len(data.art_events) > 3:

            #     if data.event_errors is None:
            #         data.event_errors = []

            #     data.event_errors.append(
            #         f"Art events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(data.art_events)} art events including {', '.join([event.value for event in data.art_events])}"
            #     )

        if data.creative_ministries_events is not None:
            pass
            # if len(data.creative_ministries_events) > 3:

            #     if data.event_errors is None:
            #         data.event_errors = []

            #     data.event_errors.append(
            #         f"Creative Ministry events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(data.creative_ministries_events)} creative ministry events including {', '.join([event.value for event in data.creative_ministries_events])}"
            #     )

        if data.music_events is not None:
            vocal_music = [
                EventsMusic.VOCAL_SOLO,
                EventsMusic.VOCAL_DUET,
                EventsMusic.VOCAL_SMALL_GROUP,
                EventsMusic.VOCAL_ENSEMBLE,
                EventsMusic.VOCAL_CHOIR,
            ]

            instrumental_music = [
                EventsMusic.BRASS,
                EventsMusic.KEYBOARD,
                EventsMusic.PERCUSSION,
                EventsMusic.STRINGS,
                EventsMusic.WOODWINDS,
                EventsMusic.INS_GROUP,
            ]

            if data.event_errors is None:
                data.event_errors = []

            if len(vocal_music) > 3:
                pass
                # data.event_errors.append(
                #     f"Vocal Music events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(vocal_music)} art events including {', '.join([event.value for event in vocal_music])}"
                # )

            if len(instrumental_music) > 3:
                pass
                # data.event_errors.append(
                #     f"Vocal Music events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(instrumental_music)} art events including {', '.join([event.value for event in instrumental_music])}"
                # )

        if data.individual_sports_events is not None:
            pass
            # if len(data.individual_sports_events) > 3:

            #     if data.event_errors is None:
            #         data.event_errors = []

            #     data.event_errors.append(
            #         f"Creative Ministry events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(data.individual_sports_events)} individual sport events including {', '.join([event.value for event in data.individual_sports_events])}"
            #     )

        if len(data.team_sports_events) > 0:
            # bracketed events
            pass
            # if len(data.team_sports_events) > 3:

            #     if data.event_errors is None:
            #         data.event_errors = []

            #     data.event_errors.append(
            #         f"Creative Ministry events cannot exceed 3: {data.first_name} {data.last_name} is registered for {len(data.team_sports_events)} team sport events including {', '.join([event.value for event in data.team_sports_events])}"
            #     )

        return data
