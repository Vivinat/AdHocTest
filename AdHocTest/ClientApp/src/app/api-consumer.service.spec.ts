import { TestBed } from '@angular/core/testing';

import { ApiConsumerService } from './api-consumer.service';

describe('ApiConsumerService', () => {
  let service: ApiConsumerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ApiConsumerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
