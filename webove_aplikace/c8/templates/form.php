<!--p class="alert alert-danger" role="alert">
	Some form items are not valid. Verify your inputs and submit again.
</p-->

<form action="?" method="GET">
	<div class="row">
		<div class="form-group col-md-6">
			<label for="fullName">Full Name:</label>
			<input class="form-control" type="text" id="fullName" name="fullName" maxlength="100" value="">
		</div>
		<div class="form-group col-md-3">
			<label for="age">Age:</label>
			<input class="form-control" type="number" id="age" name="age" min="1" max="200" maxlength="3" value="">
		</div>
		<div class="form-group col-md-3">
			<div class="form-check">
				<input class="form-check-input" type="radio" id="sexM" name="sex" value="M">
				<label class="form-check-label" for="sexM">Male</label>
			</div>
			<div class="form-check">
				<input class="form-check-input" type="radio" id="sexF" name="sex" value="F">
				<label class="form-check-label" for="sexF">Female</label>
			</div>
			<div class="form-check">
				<input class="form-check-input" type="radio" id="sexO" name="sex" value="O" checked>
				<label class="form-check-label" for="sexO">Other</label>
			</div>
		</div>
	</div>

	<div class="row">
		<div class="form-group col-md-12">
			<label>Children:</label>
			<input class="form-control" type="text" id="children" name="children" maxlength="256" value="">
		</div>
	</div>

	<div class="form-group text-center mt-4">
		<button type="submit" class="btn btn-primary">Submit</button>
	</div>
</form>
