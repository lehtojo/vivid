section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h

extern function_allocate
extern function_integer_power


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 36
push dword 8
call function_allocate
add esp, 4
push eax
push S1
push eax
call type_string_constructor
add esp, 8
pop eax
mov dword [ebp-4], eax

push dword 8
call function_allocate
add esp, 4
push eax
push S2
push eax
call type_string_constructor
add esp, 8
pop eax
mov dword [ebp-12], eax

push eax
push dword [ebp-4]
call type_string_function_combine_0
add esp, 8
mov dword [ebp-16], eax

push eax
call function_println_0
add esp, 4

push dword 8
call function_allocate
add esp, 4
push eax
push S3
push eax
call type_string_constructor
add esp, 8
pop eax
push eax
push eax
push eax
push dword 16
push dword 2
call function_integer_power
add esp, 8
pop ebx
push eax
call function_to_string
add esp, 4
pop ebx
push eax
push ebx
call type_string_function_combine_0
add esp, 8
pop ebx
push eax
call function_println_0
add esp, 4

call function_readln
mov dword [ebp-8], eax

push eax
call type_string_function_length
add esp, 4
cmp eax, dword 10
jle function_run_L2
push S5
call function_println_1
add esp, 4
jmp function_run_L1
function_run_L2:
push S4
call function_println_1
add esp, 4
function_run_L1: 

push dword [ebp-8]
call function_print_0
add esp, 4

mov dword [ebp-24], dword 0

mov dword [ebp-28], dword 1

mov dword [ebp-36], dword 0

mov dword [ebp-32], dword 0

mov dword [ebp-20], dword -6

push dword [ebp-20]
call function_to_string
add esp, 4
push eax
call function_println_0
add esp, 4

mov eax, dword [ebp-20]
mov ebx, dword 4
xor edx, edx
idiv ebx
push edx
call function_to_string
add esp, 4
push eax
call function_println_0
add esp, 4

function_run_L3:
mov eax, dword [ebp-32]
cmp eax, dword 100
jge function_run_L4
mov ebx, dword [ebp-24]
add ebx, dword [ebp-28]
mov dword [ebp-36], ebx
mov ecx, dword [ebp-28]
mov dword [ebp-24], ecx
mov dword [ebp-28], ebx
push dword [ebp-36]
call function_to_string
add esp, 4
push eax
call function_println_0
add esp, 4
mov eax, dword [ebp-32]
add eax, dword 1
mov dword [ebp-32], eax
jmp function_run_L3
function_run_L4:

function_run_L5:
mov dword [ebp-4], dword 0
function_run_L6:
mov eax, dword [ebp-4]
cmp eax, dword 10
jge function_run_L7
push eax
call function_to_string
add esp, 4
push eax
call function_println_0
add esp, 4
mov eax, dword [ebp-4]
add eax, dword 1
mov dword [ebp-4], eax
jmp function_run_L6
function_run_L7:
call function_readln
jmp function_run_L5

mov esp, ebp
pop ebp
ret


extern function_copy_0


extern function_copy_1


; Member function '' of type 'string'
type_string_constructor:
push ebp
mov ebp, esp
mov eax, dword [ebp+12]
mov edi, dword [ebp+8]
mov dword [edi], eax

mov esp, ebp
pop ebp
ret

; Member function 'combine' of type 'string'
type_string_function_combine_0:
push ebp
mov ebp, esp
sub esp, 12
push dword [ebp+8]
call type_string_function_length
add esp, 4
mov dword [ebp-4], eax

push dword [ebp+12]
call type_string_function_length
add esp, 4
add eax, dword 1
mov dword [ebp-8], eax

mov ebx, dword [ebp-4]
add ebx, eax
push ebx
call function_allocate
add esp, 4
mov dword [ebp-12], eax

push eax
push dword [ebp-4]
mov esi, dword [ebp+8]
push dword [esi]
call function_copy_0
add esp, 12

push dword [ebp-4]
push dword [ebp-12]
push dword [ebp-8]
mov eax, dword [ebp+12]
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

; Member function 'combine' of type 'string'
type_string_function_combine_1:
push ebp
mov ebp, esp
sub esp, 8
push dword [ebp+8]
call type_string_function_length
add esp, 4
mov dword [ebp-8], eax

add eax, dword 2
push eax
call function_allocate
add esp, 4
mov dword [ebp-4], eax

push eax
push dword [ebp-8]
mov esi, dword [ebp+8]
push dword [esi]
call function_copy_0
add esp, 12

mov eax, dword [ebp+12]
mov ebx, dword [ebp-8]
mov ecx, dword [ebp-4]
lea edx, [ecx+ebx*1]
mov byte [edx], al

add ebx, dword 1
lea esi, [ecx+ebx*1]
mov byte [esi], byte 0

push dword 8
call function_allocate
add esp, 4
push eax
push dword [ebp-4]
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

; Member function 'insert' of type 'string'
type_string_function_insert:
push ebp
mov ebp, esp
sub esp, 8
push dword [ebp+8]
call type_string_function_length
add esp, 4
mov dword [ebp-8], eax

add eax, dword 2
push eax
call function_allocate
add esp, 4
mov dword [ebp-4], eax

push eax
push dword [ebp+12]
mov esi, dword [ebp+8]
push dword [esi]
call function_copy_0
add esp, 12

mov eax, dword [ebp+12]
add eax, dword 1
push eax
push dword [ebp-4]
mov eax, dword [ebp-8]
sub eax, dword [ebp+12]
push eax
mov esi, dword [ebp+8]
push dword [esi]
call function_copy_1
add esp, 16

mov eax, dword [ebp+16]
mov ebx, dword [ebp+12]
mov ecx, dword [ebp-4]
lea edx, [ecx+ebx*1]
mov byte [edx], al

mov esi, dword [ebp-8]
add esi, dword 1
lea edi, [ecx+esi*1]
mov byte [edi], byte 0

push dword 8
call function_allocate
add esp, 4
push eax
push dword [ebp-4]
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
mov esi, dword [ebp+8]
mov eax, dword [esi]
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
mov dword [ebp-4], dword 0

type_string_function_length_L1:
mov eax, dword [ebp-4]
mov esi, dword [ebp+8]
mov ebx, dword [esi]
lea ecx, [ebx+eax*1]
cmp byte [ecx], byte 0
jne type_string_function_length_L2
mov esp, ebp
pop ebp
ret
type_string_function_length_L2: 
mov eax, dword [ebp-4]
add eax, dword 1
mov dword [ebp-4], eax
jmp type_string_function_length_L1

mov esp, ebp
pop ebp
ret


; Represents global function 'length_of'
function_length_of:
push ebp
mov ebp, esp
sub esp, 4
mov dword [ebp-4], dword 0

function_length_of_L1:
mov eax, dword [ebp-4]
mov ebx, dword [ebp+8]
lea ecx, [ebx+eax*1]
cmp byte [ecx], byte 0
jne function_length_of_L2
mov esp, ebp
pop ebp
ret
function_length_of_L2: 
mov eax, dword [ebp-4]
add eax, dword 1
mov dword [ebp-4], eax
jmp function_length_of_L1

mov esp, ebp
pop ebp
ret


; Represents global function 'to_string'
function_to_string:
push ebp
mov ebp, esp
sub esp, 12
push dword 8
call function_allocate
add esp, 4
push eax
push S6
push eax
call type_string_constructor
add esp, 8
pop eax
mov dword [ebp-4], eax

push dword 8
call function_allocate
add esp, 4
push eax
push S7
push eax
call type_string_constructor
add esp, 8
pop eax
mov dword [ebp-8], eax

mov ebx, dword [ebp+8]
cmp ebx, dword 0
jge function_to_string_L1
push dword 8
call function_allocate
add esp, 4
push eax
push S8
push eax
call type_string_constructor
add esp, 8
pop eax
mov dword [ebp-8], eax
mov ebx, dword 0
sub ebx, dword [ebp+8]
mov dword [ebp+8], ebx
function_to_string_L1: 


function_to_string_L2:
mov eax, dword [ebp+8]
mov ebx, dword 10
xor edx, edx
idiv ebx
mov dword [ebp-12], edx
mov eax, dword [ebp+8]
mov ecx, dword 10
xor edx, edx
idiv ecx
mov dword [ebp+8], eax
mov edx, dword 48
add edx, dword [ebp-12]
push edx
push dword 0
push dword [ebp-4]
call type_string_function_insert
add esp, 12
mov dword [ebp-4], eax
mov ebx, dword [ebp+8]
cmp ebx, dword 0
jne function_to_string_L3
push eax
push dword [ebp-8]
call type_string_function_combine_0
add esp, 8
mov esp, ebp
pop ebp
ret
function_to_string_L3: 
jmp function_to_string_L2

mov esp, ebp
pop ebp
ret


extern function_sys_print


extern function_sys_read


; Represents global function 'print'
function_print_0:
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
call function_sys_print
add esp, 8

mov esp, ebp
pop ebp
ret


; Represents global function 'print'
function_print_1:
push ebp
mov ebp, esp
push dword [ebp+8]
call function_length_of
add esp, 4
push eax
push dword [ebp+8]
call function_sys_print
add esp, 8

mov esp, ebp
pop ebp
ret


; Represents global function 'println'
function_println_0:
push ebp
mov ebp, esp
push dword [ebp+8]
call type_string_function_length
add esp, 4
add eax, dword 1
push eax
push dword 10
push dword [ebp+8]
call type_string_function_combine_1
add esp, 8
push eax
push eax
call type_string_function_data
add esp, 4
pop ebx
push eax
call function_sys_print
add esp, 8

mov esp, ebp
pop ebp
ret


; Represents global function 'println'
function_println_1:
push ebp
mov ebp, esp
push dword 8
call function_allocate
add esp, 4
push eax
push dword [ebp+8]
push eax
call type_string_constructor
add esp, 8
pop eax
push eax
push dword 10
push eax
call type_string_function_combine_1
add esp, 8
pop ebx
push eax
call function_println_0
add esp, 4

mov esp, ebp
pop ebp
ret


; Represents global function 'readln'
function_readln:
push ebp
mov ebp, esp
sub esp, 4
push dword 256
call function_allocate
add esp, 4
mov dword [ebp-4], eax

push dword 256
push eax
call function_sys_read
add esp, 8

push dword 8
call function_allocate
add esp, 4
push eax
push dword [ebp-4]
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




section .data
S1 db 'Hello ', 0
S2 db 'World!', 0
S3 db 'Power: ', 0
S4 db '<= 10', 0
S5 db '> 10', 0
S6 db '', 0
S7 db '', 0
S8 db '-', 0


