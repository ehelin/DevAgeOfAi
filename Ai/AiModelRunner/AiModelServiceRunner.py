"""
Command-line interface for the AI model.
Supports both interactive CLI mode and stdin/stdout mode for C# integration.
Uses the shared ai_model_core for model functionality.
"""

import sys
import warnings

# Import the shared model core
from ai_model_core import get_model_instance


def main_c_sharp_mode(model_core):
    """Handles interaction with C# via stdin/stdout."""
    print("Python model ready")  # Signal to C# that Python is ready
    sys.stdout.flush()

    while True:
        try:
            input_line = sys.stdin.readline().strip()

            # Ignore empty input (prevents hanging)
            if not input_line:
                continue  

            # Exit condition
            if input_line.lower() == "exit":
                print("Python exiting")
                sys.stdout.flush()
                break

            # Generate response
            response = model_core.generate_response(input_line)

            # Print response for C# to read
            print(response)
            sys.stdout.flush()

        except Exception as e:
            print(f"Error: {str(e)}")
            sys.stdout.flush()


def main_interactive_mode(model_core):
    """Interactive command-line mode for direct user interaction."""
    print("Running in interactive mode. Type 'exit' to quit.")
    print("\nTip: Try asking 'Please suggest a habit that can be tracked'\n")
    
    seen_responses = set()  # Track unique responses
    
    while True:
        try:
            input_text = input("You: ")
            if input_text.lower() == "exit":
                print("Exiting interactive mode.")
                break
            
            # Generate response
            response = model_core.generate_response(input_text)
            
            # Handle duplicate responses
            if response not in seen_responses:
                print("Model:", response)
                seen_responses.add(response)
            else:
                # Try again with slightly different temperature
                attempts = 0
                while response in seen_responses and attempts < 3:
                    attempts += 1
                    # Generate with higher temperature for variety
                    response = model_core.generate_response(
                        input_text, 
                        temperature=0.7 + (attempts * 0.1)
                    )
                
                if response not in seen_responses:
                    print("Model:", response)
                    seen_responses.add(response)
                else:
                    # If still duplicate, show it anyway
                    print("Model:", response)
                    
        except KeyboardInterrupt:
            print("\nExiting interactive mode.")
            break
        except Exception as e:
            print(f"Error: {str(e)}")


def main():
    """Main entry point that determines the mode and runs accordingly."""
    # Suppress warnings
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        
        print("Starting Python AI Model Service...")
        
        # Initialize the model (singleton)
        model_core = get_model_instance()
        
        # Get model info
        model_info = model_core.get_model_info()
        print(f"Model loaded: {model_info['model_name']}")
        print(f"LoRA adapters: {'Loaded' if model_info['adapter_loaded'] else 'Not found'}")
        
        # Determine mode based on whether stdin is a terminal
        if sys.stdin.isatty():  
            # Running in terminal - interactive mode
            main_interactive_mode(model_core)
        else:  
            # Running from C# - stdin/stdout mode
            print("Running in C# integration mode.")
            sys.stdout.flush()
            main_c_sharp_mode(model_core)


if __name__ == "__main__":
    main()