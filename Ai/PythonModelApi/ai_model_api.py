from fastapi import FastAPI, Request
from pydantic import BaseModel
import torch
import re
from transformers import AutoTokenizer, AutoModelForCausalLM
import uvicorn

print("DEBUG: Loading model...")

# model_path = "./fine_tuned_phi_v3/"
model_path = "C:/temp/DevAgeTraining/Ai/AiModelRunner/fine_tuned_phi_v3/"
tokenizer = AutoTokenizer.from_pretrained(model_path)
model = AutoModelForCausalLM.from_pretrained(model_path)

print("Model loaded.")

# --- FastAPI app setup ---
app = FastAPI()

# --- Request schema ---
class PromptInput(BaseModel):
    prompt: str

# --- Output filtering ---
def filter_output(response, promptWithInput):
    response = response.split("Habit =")[-1].strip()
    response = response.replace("\\", "")
    for token in ["\\", "\"", "'", "�", "*", "=", "•", "—"]:
        response = response.replace(token, "")
    response = re.sub(r"\s+", " ", response)
    response = re.sub(r"^(a\)|b\)|c\)|-+|\d+\.)", "", response, flags=re.IGNORECASE)
    for end_marker in ["support", "output", "frequency", "goal", "purpose"]:
        index = response.lower().find(end_marker)
        if index > 0:
            response = response[:index]
    response = re.sub(r"[^\w\s]", "", response)
    words = response.strip().split()
    response = " ".join(words)
    return response.strip()

# --- Generation logic ---
def generate_response(promptWithInput):
    inputs = tokenizer(promptWithInput, return_tensors="pt")
    inputs = {k: v.to(model.device) for k, v in inputs.items()}

    outputs = model.generate(
        **inputs,
        max_new_tokens=30,
        do_sample=True,
        top_k=50,
        top_p=0.9,
        temperature=0.7,
        no_repeat_ngram_size=4,
        pad_token_id=tokenizer.eos_token_id,
        bad_words_ids=[[tokenizer(word).input_ids[0]] for word in ["How", "Why", "What", "When", "Where", "Who", "?"]],
        repetition_penalty=2.0
    )

    response = tokenizer.decode(outputs[0], skip_special_tokens=True).strip()
    response = response.replace("\n", " ").replace("\r", " ").strip()
    response = re.sub(r"\s+", " ", response)

    filtered = filter_output(response, promptWithInput)
    return f"Start: {filtered} :End"

# --- API Endpoint ---
@app.post("/generate")
async def generate(prompt_input: PromptInput):
    try:
        result = generate_response(prompt_input.prompt)
        return {"response": result}
    except Exception as e:
        return {"error": str(e)}

# --- Entry point ---
if __name__ == "__main__":
    try:
        uvicorn.run(app, host="127.0.0.1", port=8000)
    except Exception as e:
        test = {"error": str(e)}

