FillMaskSprite. 
Перебирает все префабы в проекте и детей внутри и ищет SpriteMaskChangeRuntime (или отдельные спрайтрендереры и маски).
В SpriteMaskChangeRuntime по имени файла в массиве ищет ему маску. При этом поля листов со спрайтома и масками приватный.
После этого сохраняет изменение. EditorUtility.SetDirty() нужен для пометки что были изменения, так как если на корневом ГО ничего не поменяется, а внутри ребенка да, то Юнити не видит что были изменения.
Также во избежание ошибок в консоле префабы ВАРИАНТЫ не меняются.
З.Ы. SpriteMaskChangeRuntime не мой код )
