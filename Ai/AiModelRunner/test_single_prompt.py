from transformers import AutoTokenizer, AutoModelForCausalLM
import torch

# Load model
model_name = "microsoft/Phi-3.5-mini-instruct"
print(f"Loading {model_name}...")

tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    torch_dtype=torch.bfloat16 if torch.cuda.is_available() else torch.float32,
    device_map="auto"
)

# Test prompt
test_prompt = "Please suggest a habit that can be tracked"

print(f"\nTesting with prompt: '{test_prompt}'")
print("-" * 60)

# Method 1: With system prompt (RECOMMENDED)
print("\nMethod 1: With System Prompt (Recommended)")
messages = [
    {
        "role": "system", 
        "content": "You are a habit tracking assistant. Provide specific, actionable, trackable habits. Examples: 'Walk 10000 steps daily', 'Drink 8 glasses of water', 'Meditate for 10 minutes'. Reply with only the habit, no explanation."
    },
    {
        "role": "user", 
        "content": test_prompt
    }
]

prompt = tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
inputs = tokenizer(prompt, return_tensors="pt").to(model.device)

outputs = model.generate(
    **inputs,
    max_new_tokens=30,
    temperature=0.7,
    do_sample=True,
    top_p=0.9,
    pad_token_id=tokenizer.pad_token_id,
    eos_token_id=tokenizer.eos_token_id,
)

response = tokenizer.decode(outputs[0], skip_special_tokens=True)
if "<|assistant|>" in response:
    response = response.split("<|assistant|>")[-1].strip()

print(f"Response: {response}")

# Method 2: Direct prompt (less effective)
print("\n" + "-" * 60)
print("\nMethod 2: Direct Prompt (Current implementation)")

direct_prompt = test_prompt
inputs = tokenizer(direct_prompt, return_tensors="pt").to(model.device)

outputs = model.generate(
    **inputs,
    max_new_tokens=30,
    num_beams=5,
    do_sample=True,
    temperature=0.7,
    top_p=0.9,
)

response = tokenizer.decode(outputs[0], skip_special_tokens=True)
response = response[len(direct_prompt):].strip()

print(f"Response: {response}")

# Method 3: With few-shot examples
print("\n" + "-" * 60)
print("\nMethod 3: Few-shot Learning")

few_shot_prompt = """Examples of trackable habits:
- Walk 10000 steps daily
- Drink 8 glasses of water
- Read for 30 minutes before bed
- Meditate for 10 minutes each morning

Please suggest a habit that can be tracked:"""

inputs = tokenizer(few_shot_prompt, return_tensors="pt").to(model.device)

outputs = model.generate(
    **inputs,
    max_new_tokens=30,
    temperature=0.7,
    do_sample=True,
    top_p=0.9,
)

response = tokenizer.decode(outputs[0], skip_special_tokens=True)
response = response[len(few_shot_prompt):].strip()

print(f"Response: {response}")

print("\n" + "=" * 60)
print("RECOMMENDATION: Use Method 1 (System Prompt) for best results")
print("=" * 60)