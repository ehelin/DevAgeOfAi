"""
Core AI Model functionality that can be reused across different interfaces:
- Command line (AiModelServiceRunner.py)
- Flask API (ai_model_server.py)
- C# tests (ai_model_test_wrapper.py)
"""

import os
import warnings
import logging
import contextlib
import io
from typing import Optional, Dict, Any

# Suppress all warnings
warnings.filterwarnings('ignore')
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

# Suppress transformers and other library logging
logging.getLogger('transformers').setLevel(logging.ERROR)
logging.getLogger('transformers.modeling_utils').setLevel(logging.ERROR) 
logging.getLogger('transformers.tokenization_utils').setLevel(logging.ERROR)
logging.getLogger('peft').setLevel(logging.ERROR)
logging.getLogger('bitsandbytes').setLevel(logging.ERROR)


class AiModelCore:
    """Core AI model class that handles model loading and text generation."""
    
    def __init__(self, 
                 model_name: str = "microsoft/Phi-3.5-mini-instruct",
                 adapter_path: str = "./fine_tuned_phi_habits",
                 device: str = "cpu",
                 max_new_tokens: int = 10,
                 temperature: float = 0.5,
                 top_p: float = 0.9):
        """
        Initialize the AI model core.
        
        Args:
            model_name: HuggingFace model identifier
            adapter_path: Path to LoRA adapters (optional)
            device: Device to run on ('cpu' or 'cuda')
            max_new_tokens: Maximum tokens to generate
            temperature: Sampling temperature
            top_p: Nucleus sampling parameter
        """
        self.model_name = model_name
        self.adapter_path = adapter_path
        self.device = device
        self.max_new_tokens = max_new_tokens
        self.temperature = temperature
        self.top_p = top_p
        
        self.model = None
        self.tokenizer = None
        self._load_model()
    
    def _load_model(self):
        """Load the model and tokenizer with all suppressions."""
        # Import transformers with suppression
        with contextlib.redirect_stderr(io.StringIO()):
            from transformers import AutoTokenizer, AutoModelForCausalLM
            from peft import PeftModel
            import torch
            import transformers
            transformers.logging.set_verbosity_error()
        
        print(f"Loading tokenizer for {self.model_name}...")
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            self.tokenizer = AutoTokenizer.from_pretrained(self.model_name)
        
        print(f"Loading base model...")
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            with contextlib.redirect_stderr(io.StringIO()):
                self.model = AutoModelForCausalLM.from_pretrained(
                    self.model_name,
                    torch_dtype=torch.float32,  # Force float32 for CPU
                    device_map=self.device,
                    low_cpu_mem_usage=True,
                    trust_remote_code=True,  # Required for Phi-3.5
                    code_revision=None  # Use latest code revision with security patches
                )
        
        # Load LoRA adapters if available
        if os.path.exists(self.adapter_path):
            print(f"Loading LoRA adapters from {self.adapter_path}...")
            with warnings.catch_warnings():
                warnings.simplefilter("ignore")
                self.model = PeftModel.from_pretrained(self.model, self.adapter_path)
            print("LoRA adapters loaded successfully")
        else:
            print(f"No LoRA adapters found at {self.adapter_path}, using base model")
    
    def generate_response(self, input_text: str, **kwargs) -> str:
        """
        Generate a response from the model.
        
        Args:
            input_text: The input prompt
            **kwargs: Override default generation parameters
            
        Returns:
            Generated text response
        """
        try:
            # Import torch with suppression
            with contextlib.redirect_stderr(io.StringIO()):
                import torch
                import random
                import numpy as np
            
            # Build messages based on input type
            if "suggest a habit" in input_text.lower():
                messages = [
                    {"role": "user", "content": input_text}
                ]
            else:
                messages = [
                    {
                        "role": "system", 
                        "content": "You are a helpful habit tracking assistant. Provide concise, actionable responses."
                    },
                    {"role": "user", "content": input_text}
                ]
            
            # Apply chat template
            prompt = self.tokenizer.apply_chat_template(
                messages, 
                tokenize=False, 
                add_generation_prompt=True
            )
            
            # Tokenize input
            inputs = self.tokenizer(prompt, return_tensors="pt", truncation=True, max_length=512)
            
            # Move to correct device
            inputs = {k: v.to(self.model.device) for k, v in inputs.items()}
            
            # Get generation parameters
            gen_params = {
                "max_new_tokens": kwargs.get("max_new_tokens", self.max_new_tokens),
                "do_sample": kwargs.get("do_sample", True),
                "temperature": kwargs.get("temperature", self.temperature),
                "top_p": kwargs.get("top_p", self.top_p),
                "pad_token_id": self.tokenizer.pad_token_id,
                "eos_token_id": self.tokenizer.eos_token_id
            }
            
            # Generate response
            with warnings.catch_warnings():
                warnings.simplefilter("ignore")
                with torch.no_grad():
                    outputs = self.model.generate(**inputs, **gen_params)
            
            # Decode response
            generated_tokens = outputs[0][len(inputs['input_ids'][0]):]
            response = self.tokenizer.decode(generated_tokens, skip_special_tokens=True).strip()
            
            # Clean up response
            response = self._clean_response(response)
            
            return response
            
        except Exception as e:
            return f"An error occurred: {str(e)}"
    
    def _clean_response(self, response: str) -> str:
        """Clean up the generated response."""
        if not response:
            return ""
        
        # Remove quotes
        response = response.strip('"\'')
        
        # Take only first line
        if '\n' in response:
            response = response.split('\n')[0].strip()
        
        # Add punctuation if needed
        if response and not response[-1] in '.!?':
            words = response.split()
            if len(words) >= 3:
                response = response + '.'
        
        return response.strip()
    
    def get_model_info(self) -> Dict[str, Any]:
        """Get information about the loaded model."""
        return {
            "model_name": self.model_name,
            "adapter_path": self.adapter_path,
            "adapter_loaded": os.path.exists(self.adapter_path),
            "device": self.device,
            "max_new_tokens": self.max_new_tokens,
            "temperature": self.temperature,
            "top_p": self.top_p
        }


# Singleton instance for shared use
_model_instance: Optional[AiModelCore] = None


def get_model_instance(**kwargs) -> AiModelCore:
    """
    Get or create the singleton model instance.
    
    Args:
        **kwargs: Parameters for AiModelCore initialization
        
    Returns:
        The singleton AiModelCore instance
    """
    global _model_instance
    if _model_instance is None:
        _model_instance = AiModelCore(**kwargs)
    return _model_instance


def reset_model_instance():
    """Reset the singleton model instance (useful for testing)."""
    global _model_instance
    _model_instance = None