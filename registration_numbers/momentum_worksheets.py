import json
from jotform import JotformAPIClient

from config import API_KEY, MOMENTUM_FORM_ID

if __name__ == "__main__":
    jotform = JotformAPIClient(API_KEY)
    question_data = jotform.get_form_questions(MOMENTUM_FORM_ID)

    questions = []

    for k, q in question_data.items():
        question = { 
                "label": q["name"],
                "desciption": q.get("description", None),
                "required": True if q.get("required", None) and q.get("required") == 'Yes' else False,
                "sublabel": q.get("sublabels", None),
            }

        questions.append(question)

    print(json.dumps(questions, indent=4))
