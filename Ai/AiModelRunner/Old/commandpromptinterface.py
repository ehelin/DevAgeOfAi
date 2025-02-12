import subprocess

def interact_with_command_prompt():
    # Open a subprocess with the command prompt
    cmd_process = subprocess.Popen(['cmd'], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)

    while True:
        # Read input from the command prompt
        #user_input = input("Enter your command (type 'exit' to quit): ")
        
        command_input = input("Enter your command (type 'exit' to quit): ")
        
        if command_input.lower() == 'exit':
            break
        else:
            output = 'you wrote - ' + command_input    

        # if user_input.lower() == 'exit':
        #     # If the user types 'exit', break the loop and close the subprocess
        #     break

        # Send the user input to the command prompt
        #cmd_process.stdin.write(user_input.encode() + b'\n')
        #cmd_process.stdin.flush()

        # Read the output from the command prompt
        #output = cmd_process.stdout.readline().decode().strip()

        # Print the output
        print(output)

    # Close the subprocess
    #cmd_process.stdin.close()
    #cmd_process.stdout.close()
    #cmd_process.stderr.close()
    #cmd_process.terminate()

# Call the function to start interacting with the command prompt
interact_with_command_prompt()
