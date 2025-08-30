from typing import Any

from jotform import JotformAPIClient


class JotformClient:
    def __init__(self, api_key: str, form_id: str, model_translation: Any = None):
        self.jotform_client = JotformAPIClient(api_key)
        self.form_id = form_id
        self.model_translation = model_translation

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

            registrant = self.model_translation(registrant_submission)
            processed_data.append(registrant)

        return processed_data

    def get_submission_count(self) -> int:
        return self.jotform_client.get_form(self.form_id).get("count", 0)

    def get_form_submissions(self, limit: int = 20, offset: int = 0) -> list[dict[str:str]]:
        """
        Fetches submissions for a given form ID.
        :param limit: The maximum number of submissions to fetch.
        :return: List of submissions.
        """
        # If this becomes a bottleneck, create a loop with paging and/or parallelism
        submission_count = self.get_submission_count()
        return self.jotform_client.get_form_submissions(self.form_id, limit=submission_count, offset=offset)

    def get_data(self) -> list[dict[str:str]]:
        data = self.get_form_submissions()
        return self.parse_raw_data(data)
