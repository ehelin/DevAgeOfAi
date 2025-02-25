from transformers import pipeline

generator = pipeline("text-generation", model="gpt2")
response = generator("AI is transforming", max_length=30)
print(response[0]["generated_text"])

