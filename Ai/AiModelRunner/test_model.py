#!/usr/bin/env python
"""Test script for the fine-tuned model"""

import sys
import os
import warnings

# SECURITY: Enable all security warnings for transformers
warnings.filterwarnings("default", category=UserWarning, module="transformers")

# Add the current directory to path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Import the generate_response function from our main script
from AiModelServiceRunner import generate_response

def test_model():
    print("Testing fine-tuned model responses...\n")
    
    test_prompts = [
        "Please suggest a habit that can be tracked",
        "Please suggest a habit that can be tracked",
        "Please suggest a habit that can be tracked",
    ]
    
    for i, prompt in enumerate(test_prompts, 1):
        print(f"Test {i}: {prompt}")
        response = generate_response(prompt)
        print(f"Response: {response}\n")

if __name__ == "__main__":
    test_model()