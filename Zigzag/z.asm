function_run:
push esi
mov esi, [ebp-4]
cmp esi, 10
jge function_run_L1
function_run_L0:
push [S0]
call function_print
add esi, 1
cmp esi, 10
jl function_run_L0
function_run_L1:
pop esi
ret

function_length_of:
mov eax, [ebp-4]
mov ecx, [ebp]
function_length_of_L0:
mov edx, [ecx+eax*4]
cmp edx, 0
jne function_length_of_L1
ret
function_length_of_L1:
add eax, 1
add eax, 1
mov [ebp], ecx
mov [ebp-4], eax
jmp function_length_of_L0
ret

function_print:
push esi
mov esi, [ebp]
push esi
call function_length_of
push eax
push esi
call function_sys_print
pop esi
ret