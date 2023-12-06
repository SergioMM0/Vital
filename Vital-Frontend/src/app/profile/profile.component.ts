import {Component, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from "@angular/forms";
import {PasswordValidator} from "../validators/password.validator";
import {PasswordRules} from "../interfaces/utilities.interface";
import {CycleService} from "../services/cycle.service";
import {UserService} from "../services/user.service";
import {PeriodCycleStatsDto} from "../interfaces/analytics.interface";

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html'
})

export class ProfileComponent implements OnInit{

  public periodCycleStats: PeriodCycleStatsDto | undefined;
  public userEmail: string = '';
  constructor(private cycleService: CycleService, private userService: UserService) {
    this.subscribeToPasswordChanges();
  }

  async ngOnInit(): Promise<void>{
    this.periodCycleStats = await this.cycleService.getUserStats();
    this.userEmail = await this.userService.getUserEmail();
  }

  passwordRulesMet: PasswordRules = {
    lengthCondition: false,
    digitCondition: false,
    lowercaseCondition: false,
    uppercaseCondition: false,
    specialCondition: false
  };

  readonly changePasswordForm = new FormGroup({
    password: new FormBuilder().control('', [Validators.required, PasswordValidator]),
    repeatPassword: new FormBuilder().control('', [Validators.required, PasswordValidator])
  }, {validators: this.passwordMatchValidator});

  passwordMatchValidator(control: AbstractControl) {
    const g = control as FormGroup;
    return g.get('password')?.value === g.get('repeatPassword')?.value
      ? null : {'mismatch': true};
  }

  subscribeToPasswordChanges(): void {
    const passwordControl: AbstractControl = this.changePasswordForm.get('password')!;

    if (passwordControl) {
      passwordControl.valueChanges.subscribe((value) => {
        this.passwordRulesMet = {
          lengthCondition: value.length >= 6,
          digitCondition: /\d/.test(value),
          lowercaseCondition: /[a-z]/.test(value),
          uppercaseCondition: /[A-Z]/.test(value),
          specialCondition: /[^a-zA-Z\d]/.test(value)
        };
      });
    }
  }
}
