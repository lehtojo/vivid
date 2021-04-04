.extern GetStdHandle
.extern WriteFile
.extern ReadConsoleA

# rcx: Address
# rdx: Length
.global _V14internal_printPhx
_V14internal_printPhx:
push rbx
push rsi
sub rsp, 56

mov rbx, rcx # Save the address
mov rsi, rdx # Save the length

mov rcx, -11
call GetStdHandle # Get the output handle

mov rcx, rax # Handle to the console output
mov rdx, rbx # Address of the buffer to write
mov r8, rsi # Size of the buffer
lea r9, qword ptr [rsp+40] # This location will contain how many characters were written
mov qword ptr [rsp+32], 0

call WriteFile

add rsp, 56
pop rsi
pop rbx
ret

# rcx: Buffer
# rdx: Length
.global _V13internal_readPhx_rx
_V13internal_readPhx_rx:
push rbx
push rsi
sub rsp, 56

mov rbx, rcx # Save the buffer
mov rsi, rdx # Save the length

mov rcx, -10
call GetStdHandle # Get the input handle

mov rcx, rax # Handle to the console input
mov rdx, rbx # Address of the buffer where to store the result
mov r8, rsi # Size of the buffer
lea r9, [rsp+32] # This location will contain how many characters were read
mov qword ptr [rsp+40], 0

call ReadConsoleA

mov rax, qword ptr [rsp+32] # Return how many characters were read

add rsp, 56
pop rsi
pop rbx
ret