from pydantic import BaseModel

from .camper import Camper


class Campers(BaseModel):
    campers: list[Camper] = []

    def add_camper(self, camper: Camper):
        self.campers.append(camper)

    def get_camper_by_id(self, submission_id: str) -> Camper:
        for camper in self.campers:
            if camper.submission_id == submission_id:
                return camper
        raise ValueError(f"Camper with submission_id {submission_id} not found.")

    def remove_camper(self, submission_id: str):
        self.campers = [camper for camper in self.campers if camper.submission_id != submission_id]