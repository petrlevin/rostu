/**
* @file
*
* @brief Тестирование класса Guid
* @details Copyright (c) 2006-2013 R.O.S.T.U.
*
* @author Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
* @since $Id:$
*/

describe("Test", function() {
  it("Guid is defined in global scope", function() {
    expect(Guid).toBeDefined();
  });

  it("Guid.Empty returns Empty guid filled with zeroes", function() {
    expect(Guid.Empty).toBeDefined();
    expect(typeof(Guid.Empty)).toEqual('string');
    expect(Guid.Empty).toEqual('00000000-0000-0000-0000-000000000000');
  });

  it("Guid.isEmpty returns true if gets empty guid", function() {

		var empty = '00000000-0000-0000-0000-000000000000';
		expect(Guid.isEmpty(empty)).toBeTruthy();
	});

	it("Guid.NewGuid generates valid guids", function() {
		var reg = "^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$";
		expect(Guid.NewGuid()).toMatch(reg);
	});

	it("Guid.isValid returns true if Guid is really valid", function() {
		var guid = Guid.NewGuid();
		var someId = '00000000-0000-0000-0000-00000000000';

		expect(Guid.isValid(guid)).toBeTruthy();
		expect(Guid.isValid(someId)).toBeFalsy();
	});

	it("Guid.Equals returns true if two guids are equal, ignoring case", function() {
		var guid = Guid.NewGuid();
		var guid_one = guid.toLowerCase();
		var guid_two = guid.toUpperCase();
		expect(Guid.Equals(guid_one, guid_two)).toBeTruthy();
	});
});

