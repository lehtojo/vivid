section .text

extern GetStdHandle
extern WriteFile
extern ReadConsoleA

; rcx: Address
; rdx: Length
global _V14internal_printPhx
_V14internal_printPhx:
push rbx
push rsi

; Required spill area, 1 x 64 bit parameter and lpNumberOfCharsWritten for WriteFile (aligned)
sub rsp, 56

mov rbx, rcx ; Address
mov rsi, rdx ; Length

mov rcx, -11 ; STD_OUTPUT_HANDLE
call GetStdHandle

mov rcx, rax ; hConsoleOutput
mov rdx, rbx ; lpBuffer
mov r8, rsi ; nNumberOfCharsToWrite
lea r9, [rsp+40] ; lpNumberOfCharsWritten
mov qword [rsp+32], 0 ; lpReserved (Stack memory should be zeroes?)

call WriteFile

; Required spill area, 1 x 64 bit parameter and lpNumberOfCharsWritten for WriteFile (aligned)
add rsp, 56

pop rsi
pop rbx
ret

; rcx: Buffer
; rdx: Length
global _V13internal_readPhx_rx
_V13internal_readPhx_rx:
push rbx
push rsi

; Required spill area and 2 x 64 bit parameters for ReadConsole (aligned)
sub rsp, 56

mov rbx, rcx ; Buffer
mov rsi, rdx ; Length

mov rcx, -10 ; STD_INPUT_HANDLE

call GetStdHandle

mov rcx, rax ; hConsoleOutput
mov rdx, rbx ; lpBuffer
mov r8, rsi ; nNumberOfCharsToRead
lea r9, [rsp+32] ; lpNumberOfCharsRead
mov qword [rsp+40], 0 ; pInputControl (Stack memory should be zeroes?)

call ReadConsoleA

; Remove spill area and 2 x 64 bit parameters for ReadConsole
add rsp, 56

pop rsi
pop rbx
ret