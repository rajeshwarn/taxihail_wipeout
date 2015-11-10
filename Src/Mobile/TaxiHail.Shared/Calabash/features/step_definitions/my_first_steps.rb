Given /^I take screenshots$/ do
	sleep(2)
	if element_exists("view marked:'Book Now'")
		print('Already logged')
		signOut()
		signIn()
		screenshotHomeView()
	elsif element_exists("view marked:'Accept'")
		print('On New Terms screen')
		screenshot(prefix: "", name:"NewTerms.png")
		print(2)
		tap "Accept"
		sleep(STEP_PAUSE)
		if element_exists("view marked:'close tutorial button'")
			touch("view marked:'close tutorial button'")
		end
		sleep(2)
		signOut()
		signIn()
		wait_for(:timeout => 10) { element_exists("label text:'Book Now'") }
		screenshotHomeView()
	elsif element_exists("view marked:'close tutorial button'")
		print('On tutorial')
		screenshot(prefix: "", name:"Tutorial.png")
		print(3)
		touch("view marked:'close tutorial button'")
		sleep(2)
		if element_exists("view marked:'Accept'")
			touch("view marked:'Accept'")
		end
		sleep(2)
		signOut()
		signIn()
	elsif element_exists("view marked:'Sign In'")
		print('On Sign In')
		signIn();
		if element_exists("view marked:'close tutorial button'")
			screenshot(prefix: "", name:"Tutorial.png")
			touch("view marked:'close tutorial button'")
			sleep(2)
		end
		if element_exists("view marked:'Accept'")
			screenshot(prefix: "", name:"NewTerms.png")
			touch("view marked:'Accept'")
			sleep(2)
		end
		screenshotHomeView()
	end
end

Given /^I change the address BETA$/ do
	touch("all view:'UITextFieldLabel' {text CONTAINS ','}")
	sleep(4)
	if element_exists("view:'UITextFieldLabel'")
		touch("view marked:'close tutorial button'")
		sleep(2)
	end
	sleep(2)
	query("view:'FlatTextField'", {:setText => 'New Address'})
	sleep(2)
end

def screenshotHomeView()
	screenshot(prefix: "", name:"HomeViewLoading.png")
	wait_for(:timeout => 20) { !element_exists("view:'apcurium.MK.Booking.Mobile.Client.Controls.Message.CircularProgressView'") }
	sleep(5)
	screenshot(prefix: "", name:"HomeView.png")
	assert('true')
end

def signOut()
	tap 'menu icon'
	sleep(2)
	tap 'Sign Out'
	sleep(2)
end

def signIn()
	wait_for(:timeout => 10) { query("label text:'Sign In'") }
	sleep(2)
	screenshot(prefix: "", name:"Login.png")
	sleep(2)
	tap 'Sign In'
	wait_for(:timeout => 10) { query("label text:'Book Now'") }
	screenshotHomeView()
end