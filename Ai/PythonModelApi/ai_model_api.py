from fastapi import FastAPI
from pydantic import BaseModel
import sys
import os

# core_path = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "SharedModelCore"))
# if core_path not in sys.path:
#     sys.path.append(core_path)

# from phi_model_core import generate_response
from SharedModelCore.phi_model_core import generate_response

app = FastAPI()

class PromptInput(BaseModel):
    prompt: str

@app.post("/generate")
async def generate(prompt_input: PromptInput):
    try:
        result = generate_response(prompt_input.prompt)
        return {"response": result}
    except Exception as e:
        return {"error": str(e)}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)
