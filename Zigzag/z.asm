function_square:
push edi
push [S0]
call type_string_constructor
mov edi, eax
push 21
push edi
call type_string_function_append
push edi
call function_prints
push [S1]
push 1
push edi
call type_string_function_insert
pop edi
ret

function_run:
push 7
call function_square
ret

function_prints:
push esi
mov esi, [ebp]
push esi
call type_string_function_length
push eax
push esi
call type_string_function_data
push eax
call function_sys_print
pop esi
ret

type_string_constructor:
push 4
call allocate
mov ecx, eax
mov edx, [ebp+4]
mov [ecx], edx
ret

type_string_function_append:
push esi
push edi
mov ecx, [ebp+8]
push ecx
call type_string_function_length
mov edi, eax
lea ecx, [edi+2]
push ecx
call function_allocate
mov esi, eax
push esi
push edi
mov ecx, [ebp+8]
push [ecx]
call function_copy
mov ecx, [ebp+4]
mov [esi+edi*4], ecx
add edi, 1
mov [esi+edi*4], 0
push esi
mov ecx, [ebp+8]
push ecx
call type_string_constructor
pop edi
pop esi
ret

type_string_function_insert:
push ebx
push esi
push edi
mov ecx, [ebp+8]
push ecx
call type_string_function_length
mov edi, eax
lea ecx, [edi+2]
push ecx
call function_allocate
mov esi, eax
push esi
push [ebp+4]
mov ecx, [ebp+8]
push [ecx]
call function_copy
mov ecx, [ebp+4]
lea edx, [ecx+1]
push edx
push esi
mov edx, edi
sub edx, ecx
push edx
mov edx, [ebp+8]
push [edx]
mov ebx, ecx
call function_offset_copy
mov edx, [ebp+8]
mov [esi+ecx*4], edx
add edi, 1
mov [esi+edi*4], 0
push esi
mov ecx, [ebp+8]
push ecx
call type_string_constructor
pop edi
pop esi
pop ebx
ret

type_string_function_data:
mov ecx, [ebp+8]
mov eax, [ecx]
ret

type_string_function_length:
push ebx
mov eax, [ebp-4]
mov ecx, [ebp+8]
type_string_function_length_L0:
mov ecx, [ebp+8]
mov edx, [ecx]
mov ebx, [edx+eax*4]
cmp ebx, 0
jne type_string_function_length_L1
mov ecx, [ebp+8]
pop ebx
ret
mov [ebp-4], eax
type_string_function_length_L1:
add eax, 1
mov [ebp-4], eax
jmp type_string_function_length_L0
pop ebx
ret