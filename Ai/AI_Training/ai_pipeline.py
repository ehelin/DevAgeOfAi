from transformers import pipeline

# Load a small text-generation model
generator = pipeline("text-generation", model="gpt2")

# Run AI model inference
response = generator("What is your name", max_length=30)
print(response[0]["generated_text"])
