function_f:
push esi
mov esi, [ebp]
cmp esi, 0
je function_f_L0
mov ecx, [esi]
cmp ecx, 0
je function_f_L1
push 2
push 1
push [esi]
call type_b_function_sum
pop esi
ret
function_f_L1:
mov [ebp], esi
function_f_L0:
mov eax, 0
pop esi
ret

function_run:
push 4
call allocate
push eax
call type_a_constructor
push eax
call function_f
ret

type_b_function_sum:
mov ecx, [ebp+8]
mov eax, [ebp+4]
mov [ecx], eax
add eax, [ebp+8]
rettype_a_constructor:
push 4
call allocate
mov ecx, eax
mov edx, [ebp+4]
mov [ecx], edx
ret