//
//  Event.h
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Command.h"

@interface Event : Command

- (instancetype)initWithIds:(NSDictionary *)ids andProjectId:(NSString *)projectId andWithType:(NSString *)type
          andWithProperties:(NSDictionary *)properties andWithTimestamp:(NSNumber *)timestamp;

@end
