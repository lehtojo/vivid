section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h


extern function_allocate


extern function_print


; Represents global function 'println'
function_println:
push ebp
mov ebp, esp
push dword [ebp+8]
call type_string_function_length
add esp, 4
push eax
push dword [ebp+8]
call type_string_function_data
add esp, 4
push eax
call function_print
add esp, 8

mov esp, ebp
pop ebp
ret


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 12
push dword 8
call function_allocate
add esp, 4
push eax
push S1
push eax
call type_string_constructor
add esp, 8
pop eax
mov [ebp-4], eax

push dword 8
call function_allocate
add esp, 4
push eax
push S2
push eax
call type_string_constructor
add esp, 8
pop eax
mov [ebp-8], eax

push eax
push dword [ebp-4]
call type_string_function_combine
add esp, 8
mov [ebp-12], eax

push eax
call function_println
add esp, 4

mov esp, ebp
pop ebp
ret


extern function_allocate


extern function_copy_0


extern function_copy_1


; Member function '' of type 'string'
type_string_constructor:
push ebp
mov ebp, esp
mov eax, [ebp+12]
mov edi, [ebp+8]
mov [edi], eax

mov esp, ebp
pop ebp
ret

; Member function 'combine' of type 'string'
type_string_function_combine:
push ebp
mov ebp, esp
sub esp, 12
push dword [ebp+8]
call type_string_function_length
add esp, 4
mov [ebp-4], eax

push dword [ebp+12]
call type_string_function_length
add esp, 4
mov [ebp-8], eax

mov ebx, [ebp-4]
add ebx, eax
push ebx
call function_allocate
add esp, 4
mov [ebp-12], eax

push eax
push dword [ebp-4]
mov esi, [ebp+8]
push dword [esi]
call function_copy_0
add esp, 12

push dword [ebp-4]
push dword [ebp-12]
push dword [ebp-8]
mov eax, [ebp+12]
push dword [eax]
call function_copy_1
add esp, 16

push dword 8
call function_allocate
add esp, 4
push eax
push dword [ebp-12]
push eax
call type_string_constructor
add esp, 8
pop eax
mov esp, ebp
pop ebp
ret

mov esp, ebp
pop ebp
ret

; Member function 'data' of type 'string'
type_string_function_data:
push ebp
mov ebp, esp
mov esi, [ebp+8]
mov eax, [esi]
mov esp, ebp
pop ebp
ret

mov esp, ebp
pop ebp
ret

; Member function 'length' of type 'string'
type_string_function_length:
push ebp
mov ebp, esp
sub esp, 4
mov [ebp-4], dword 0

type_string_function_length_L1:
mov eax, [ebp-4]
mov esi, [ebp+8]
mov ebx, [esi]
lea ecx, [ebx+eax*1]
cmp [ecx], dword 0
jne type_string_function_length_L2
mov esp, ebp
pop ebp
ret
type_string_function_length_L2: 
mov eax, [ebp-4]
add eax, dword 1
mov [ebp-4], eax
jmp type_string_function_length_L1

mov esp, ebp
pop ebp
ret




section .data
S1 db 'Hello', 0
S2 db 'World!', 0


