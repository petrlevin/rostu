UPDATE ref.Programmability
SET
	isDisabled = 1
WHERE id IN (-1275068364,
-1275068363)

UPDATE ref.Programmability
SET
	isDisabled = 0
WHERE id IN (-1275068364,
-1275068363)

DELETE FROM [ref].[Entity]
WHERE id = -1879048149 