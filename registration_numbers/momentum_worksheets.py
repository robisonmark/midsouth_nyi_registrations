import json
from typing import Any, OrderedDict

from config import API_KEY, MOMENTUM_FORM_ID
from jotform import JotformAPIClient
from translators.jotform_momentum import translate_registrant


class MomentumWorksheets:
    """
    Class to handle the generation of raw data from Jotform submissions for Momentum worksheets.
    """

    def __init__(self, shirt_manager):
        self.shirt_manager = shirt_manager

    # bring this into jotform client, take in submissions and pydantic_model
    # and return a list of pydantic models
    def parse_raw_data(self, submissions: list[dict[str:str]]) -> list[dict[str:Any]]:
        """
        Generates raw data from Jotform submissions.
        :param submissions: List of Jotform submissions.
        :return: Processed data ready for further processing.
        """
        processed_data = []

        for submission in submissions:
            registrant_submission = {
                "id": submission["id"],
                "submission_date": submission["created_at"],
                "status": submission["status"],
            }

            for answer in submission["answers"].values():
                if "answer" in answer.keys():
                    registrant_submission[answer["name"]] = answer["answer"]

            registrant = translate_registrant(registrant_submission)
            # self.shirt_manager.create_individual_entry(row_data=registrant)
            processed_data.append(registrant)

        return processed_data


if __name__ == "__main__":
    momemtum_worksheets = MomentumWorksheets(shirt_manager=None)  # Replace with actual shirt manager instance
    jotform = JotformAPIClient(API_KEY)
    # question_data = jotform.get_form_questions(MOMENTUM_FORM_ID)

    # questions = []

    # for k, q in question_data.items():
    #     question = {
    #             "label": q["name"],
    #             "desciption": q.get("description", None),
    #             "required": True if q.get("required", None) and q.get("required") == 'Yes' else False,
    #             "sublabel": q.get("sublabels", None),
    #         }

    #     questions.append(question)

    # print(json.dumps(questions, indent=4))

    submissions = jotform.get_form_submissions(MOMENTUM_FORM_ID, limit=5)
    momentum_data = momemtum_worksheets.parse_raw_data(submissions)

    print(momentum_data)
