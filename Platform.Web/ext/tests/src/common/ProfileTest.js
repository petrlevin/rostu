/**
* @file
*
* @brief Синхронные тесты класса управляющего профилем пользователя
* @details Copyright (c) 2006-2013 R.O.S.T.U.
*
* @author Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
* @since $Id:$
*/

describe("ProfileTest", function () {

	beforeEach(function () {
		jasmine.Clock.useMock();

		spyOn(ProfileService, 'isLogged').andCallFake(function () { });
		spyOn(ProfileService, 'login').andCallFake(function () { });
		spyOn(ProfileService, 'logout').andCallFake(function () { });
	});

	it("ProfileService defined, Ext.Direct ready", function () {
		expect(ProfileService).toBeDefined();
		expect(ProfileService.isLogged).toBeDefined();
		expect(typeof ProfileService.isLogged).toEqual('function');
		expect(ProfileService.login).toBeDefined();
		expect(typeof ProfileService.login).toEqual('function');
		expect(ProfileService.logout).toBeDefined();
		expect(typeof ProfileService.logout).toEqual('function');
	});

	it("Calls login server method if Profile asked to login", function () {
		expect(App).toBeDefined();
		expect(App.Profile).toBeDefined();
		App.Profile.login({
			user: 'admin',
			pass: 'qwerty'
		});
		jasmine.Clock.tick(200);
		expect(ProfileService.login).toBeCalled();
	});

});